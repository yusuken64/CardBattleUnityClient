using CardBattleEngine;
using UnityEngine;
using UnityEngine.UI;

public class GameInteractionHandler : MonoBehaviour
{
	public Card CardPrefab;
	public Minion MinionPrefab;
	public Minion MinionPlayPreview;

	public PointerInput PointerInput;
	public LayerMask ClickableMask;
	private IDraggable currentDraggable;
	private ITargetOrigin currentAimable;

	public LineRenderer LineRenderer;
	public Image ArrowHead;
	public int CurveResolution = 20;
	public float CurveStrength = 2f;
	private Vector3 _aimStartPosition;
	private IDraggable pendingDraggable;

	private void OnEnable()
	{
		PointerInput.OnClick += PointerInput_OnClick;
		PointerInput.OnHoldStart += PointerInput_OnHoldStart;
		PointerInput.OnHoldEnd += PointerInput_OnHoldEnd;
		PointerInput.OnDragStart += PointerInput_OnDragStart;
		PointerInput.OnDrag += PointerInput_OnDrag;
		PointerInput.OnDragEnd += PointerInput_OnDragEnd;
	}

	private void OnDisable()
	{
		PointerInput.OnClick -= PointerInput_OnClick;
		PointerInput.OnHoldStart -= PointerInput_OnHoldStart;
		PointerInput.OnHoldEnd -= PointerInput_OnHoldEnd;
		PointerInput.OnDragStart -= PointerInput_OnDragStart;
		PointerInput.OnDrag -= PointerInput_OnDrag;
		PointerInput.OnDragEnd -= PointerInput_OnDragEnd;
	}

	private void PointerInput_OnDragStart(Vector2 obj)
	{
		var mousePos = GetMouseWorldPosition2D(obj);

		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
		if (hit.collider != null)
		{
			var draggable = hit.collider.GetComponent<IDraggable>();
			if (draggable != null &&
				draggable.CanStartDrag())
			{
				draggable.Dragging = true;
				currentDraggable = draggable;
			}

			var targetOrigin = hit.collider.GetComponent<ITargetOrigin>();
			if (targetOrigin != null &&
				targetOrigin.CanStartAiming())
			{
				currentAimable = targetOrigin;
				StartLine(currentAimable.DragObject.transform.position);
			}
		}
	}

	private void PointerInput_OnDrag(Vector2 obj)
	{
		var mousePos = GetMouseWorldPosition2D(obj);
		if (currentDraggable != null)
		{
			currentDraggable.DragObject.transform.position = mousePos;
			var isMouseOverBoard = MouseOverBoard(mousePos).collider != null;
			currentDraggable.PreviewPlayOverBoard(mousePos, isMouseOverBoard);

			if (isMouseOverBoard)
			{
				if (currentDraggable.RequiresTarget())
				{
					//invoke targeting
					//start aiming
					var pendingAimable = currentDraggable.TransitionToAim(mousePos);
					currentAimable = pendingAimable.GetComponent<ITargetOrigin>();
					pendingDraggable = currentDraggable;
					currentDraggable = null;
					StartLine(currentAimable.DragObject.transform.position);
				}
			}
		}
		else if (currentAimable != null)
		{
			UpdateAimingLine(_aimStartPosition, mousePos);
			//UpdateArrowHead();
		}
	}

	private void PointerInput_OnDragEnd(Vector2 obj)
	{
		var mousePos = GetMouseWorldPosition2D(obj);
		if (currentDraggable != null)
		{
			currentDraggable.DragObject.transform.position = mousePos;
			currentDraggable.Dragging = false;

			if (MouseOverBoard(mousePos).collider != null &&
				currentDraggable.CanResolve(mousePos, out var current))
			{
				currentDraggable.Resolve(mousePos, current);
				CancelAim();
			}
			else
			{
				currentDraggable.CancelDrag();
				CancelAim();
			}

			currentDraggable = null;
		}
		else if (currentAimable != null)
		{
			RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
			ITargetable target = null;
			if (hit.collider != null)
			{
				target = hit.collider.GetComponent<ITargetable>();
				if (target == null)
				{
					target = hit.collider.GetComponentInParent<ITargetable>();
				}
			}

			if (target != null &&
				currentAimable.WillResolveSuccessfully(target, pendingDraggable?.DragObject, out var current, mousePos))
			{
				currentAimable.ResolveAim(current, pendingDraggable?.DragObject);
				EndAim();
			}
			else
			{
				//cancel the action
				CancelAim();
			}
		}
	}

	private void CancelAim()
	{
		if (pendingDraggable != null)
		{
			pendingDraggable?.CancelAim();
		}

		EndAim();
	}

	private void EndAim()
	{
		if (pendingDraggable != null)
		{
			pendingDraggable?.EndAim();
		}

		pendingDraggable = null;
		currentAimable = null;
		currentDraggable = null;

		EndLine();
	}

	private void PointerInput_OnHoldEnd(Vector2 obj)
	{
		//hide info
	}

	private void PointerInput_OnHoldStart(Vector2 obj)
	{
		//show info
	}

	private void PointerInput_OnClick(Vector2 obj)
	{
		var mousePos = GetMouseWorldPosition2D(obj);

		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
		if (currentAimable != null)
		{
			var target = hit.collider.GetComponent<ITargetable>();
			//currentAimable.ResolveTarget(target);
		}
		else if (hit.collider != null)
		{
			var targetOrigin = hit.collider.GetComponent<ITargetOrigin>();
			if (targetOrigin != null)
			{
				if (targetOrigin.CanStartAiming())
				{
					currentAimable = targetOrigin;
				}
				else
				{
					//currentAimable.ResolveTarget(null);
				}
			}
		}
	}

	private Vector3 GetMouseWorldPosition2D(Vector3 mousePos)
	{
		Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
		world.z = 0;
		return world;
	}

	private RaycastHit2D MouseOverBoard(Vector2 mousePos)
	{
		int layerMask = LayerMask.GetMask("PlayArea");

		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, layerMask);
		return hit;
	}

	// --- Line Rendering ---
	private void StartLine(Vector3 start)
	{
		_aimStartPosition = start;
		LineRenderer.enabled = true;
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
}
public interface ITargetOrigin
{
	public AimIntent AimIntent { get; set; }
	bool CanStartAiming();
	GameObject DragObject { get; }
	CardBattleEngine.IGameEntity GetData();
	CardBattleEngine.Player GetPlayer();
	void ResolveAim((IGameAction action, ActionContext context) current, GameObject dragObject);
	bool WillResolveSuccessfully(ITargetable target, GameObject pendingAimObject, out (IGameAction, ActionContext) current, Vector3 mousePos);
}

public enum AimIntent
{
	Attack,
	CastSpell,
	HeroPower
}

public interface ITargetable
{
	CardBattleEngine.IGameEntity GetData();
}

public interface IDraggable
{
	GameObject DragObject { get; }
	bool Dragging { get; set; }

	bool CanStartDrag();
	void PreviewPlayOverBoard(Vector3 mousePos, bool mouseOverBoard);
	bool CanResolve(Vector3 mousePos, out (IGameAction action, ActionContext context) current);
	void Resolve(Vector3 mousePos, (IGameAction action, ActionContext context) current);
	void CancelDrag();
	bool RequiresTarget();
	GameObject TransitionToAim(Vector3 mousePos);
	void CancelAim();
	void EndAim();
}