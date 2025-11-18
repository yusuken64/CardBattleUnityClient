using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CardInteractionController : MonoBehaviour
{
    public LineRenderer LineRenderer;
    public Image ArrowHead;
    public LayerMask ClickableMask;
    public int CurveResolution = 20;
    public float CurveStrength = 2f;
    public RectTransform BoardArea;

    private Transform activeThing = null; // the clicked object
    private Vector3 originalPosition;
    private bool dragging = false;
    private bool isAiming = false;

    // --- Delegates ---
    public delegate void CardPlayedHandler(Player player, Card card, int index);
    public delegate void SpellPlayedHandler(Player player, Card card);
    public delegate void MinionPlayedPreviewdHandler(Player player, Card card, int index);
    public delegate void TargetSelectedHandler(ITargetOrigin source, ITargetable target);
    public delegate void TargetingCanceledHandler();

    // --- Events ---
    public event CardPlayedHandler CardPlayed;
    public event SpellPlayedHandler SpellPlayedPreview;
    public event MinionPlayedPreviewdHandler MinionPlayedPreview;
    public event TargetSelectedHandler TargetSelected;
    public event TargetingCanceledHandler TargetingCanceled;

    void Update()
    {
        Vector3 mousePos = GetMouseWorldPosition2D();

        // --- Mouse press ---
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
            if (hit.collider != null)
            {
                activeThing = hit.transform;

                // Determine mode: draggable or aimable
                if (activeThing.TryGetComponent<IDraggable>(out var draggable) &&
                    draggable.CanStartDrag()) //TODO replace with get draggable
				{
					StartDragging(activeThing);
				}
				else if (activeThing.TryGetComponent<ITargetOrigin>(out var targetOrigin) &&
                    targetOrigin.CanStartAiming())
				{
					StartAiming(activeThing);
				}
			}
        }

        // --- Dragging ---
        if (dragging && activeThing != null)
        {
            activeThing.position = mousePos;

            if (MouseOverBoard().collider != null) {
                var card = activeThing.GetComponent<Card>();
                if (card.CardType == CardBattleEngine.CardType.Spell &&
					card.RequiresTarget)
                {
                    dragging = false;
                    activeThing = null;
                    var player = FindFirstObjectByType<GameManager>().Player;
                    SpellPlayedPreview?.Invoke(player, card);
                }

                if (card.CardType == CardBattleEngine.CardType.Minion)
                {
                    var player = FindFirstObjectByType<GameManager>().Player;
                    var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);
                    MinionPlayedPreview?.Invoke(player, card, index);
                }
			}
			else
            {
                var card = activeThing.GetComponent<Card>();
                if (card.CardType == CardBattleEngine.CardType.Minion)
                {
                    var player = FindFirstObjectByType<GameManager>().Player;
                    MinionPlayedPreview?.Invoke(player, card, -1);
                }
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                ReleaseDraggedThing(mousePos);
            }
        }

        // --- Aiming ---
        else if (isAiming && activeThing != null)
        {
            UpdateAimingLine(activeThing.position, mousePos);
            UpdateArrowHead();

            // Only trigger target selection on *release*, not on the initial click
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                RaycastHit2D targetHit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
                if (targetHit.collider != null)
                {
                    Debug.Log($"Target selected: {activeThing.name} -> {targetHit.transform.name}");
                    TargetSelected?.Invoke(
                        activeThing.GetComponent<ITargetOrigin>(),
                        targetHit.collider.GetComponent<ITargetable>());
				}
				else
				{
                    //TODO check it itargetable.cantarget here
                    TargetingCanceled?.Invoke();
				}
                EndLine();
                activeThing = null;
                isAiming = false;
            }
        }

    }

	public void StartAiming(Transform newActiveThing)
	{
        activeThing = newActiveThing;
		isAiming = true;
		StartLine(activeThing.position);
	}

    public void StartDragging(Transform activeThing)
	{
        var dragggable = activeThing.GetComponent<IDraggable>();
        dragggable.Dragging = true;
		dragging = true;
		originalPosition = activeThing.position;
	}

	private void ReleaseDraggedThing(Vector3 mousePos)
	{
		RaycastHit2D hit = MouseOverBoard();

        var dragggable = activeThing.GetComponent<IDraggable>();
        dragggable.Dragging = false;
        if (hit.collider != null)
		{
			Debug.Log($"Played {activeThing.name}");
            var player = FindFirstObjectByType<GameManager>().Player;
            var card = activeThing.GetComponent<Card>();
            dragging = false;
            activeThing = null;

            var index = player.Board.Minions.Count(x => x.transform.position.x < mousePos.x);
            CardPlayed?.Invoke(player, card, index);
        }
		else
		{
            //activeThing.position = originalPosition;
            var card = activeThing.GetComponent<Card>();
            card.Dragging = false;
            card.Moving = true;
            dragging = false;
            activeThing = null;
            TargetingCanceled?.Invoke();
        }
	}

	private RaycastHit2D MouseOverBoard()
	{
		int layerMask = LayerMask.GetMask("PlayArea");

		Vector3 mousePos = GetMouseWorldPosition2D();
		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
		return hit;
	}

	// --- Line Rendering ---
	private void StartLine(Vector3 start)
    {
        LineRenderer.enabled = true;
        LineRenderer.positionCount = 2;
        LineRenderer.SetPosition(0, start);
        LineRenderer.SetPosition(1, start);
    }

    private void UpdateAimingLine(Vector3 start, Vector3 end)
    {
        Vector3[] points = new Vector3[CurveResolution + 1];
        Vector3 mid = (start + end) * 0.5f;
        mid.y += CurveStrength;

        for (int i = 0; i <= CurveResolution; i++)
        {
            float t = i / (float)CurveResolution;
            Vector3 a = Vector3.Lerp(start, mid, t);
            Vector3 b = Vector3.Lerp(mid, end, t);
            points[i] = Vector3.Lerp(a, b, t);
        }

        LineRenderer.positionCount = points.Length;
        LineRenderer.SetPositions(points);
    }

    private void UpdateArrowHead()
    {
        if (!LineRenderer.enabled) { ArrowHead.enabled = false; return; }
        ArrowHead.enabled = true;

        int last = LineRenderer.positionCount - 1;
        Vector3 end = LineRenderer.GetPosition(last);
        Vector3 start = LineRenderer.GetPosition(last - 1);

        Vector3 dir = (end - start).normalized;

        ArrowHead.rectTransform.position = end;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        ArrowHead.rectTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private void EndLine()
    {
        LineRenderer.enabled = false;
        ArrowHead.enabled = false;
    }

    private Vector3 GetMouseWorldPosition2D()
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        world.z = 0;
        return world;
    }
}
