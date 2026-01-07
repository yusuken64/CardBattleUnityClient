using UnityEngine;

public class DeckInteractionHandler : MonoBehaviour
{
    public PointerInput PointerInput;
	public LayerMask ClickableMask;
	private IHoverable currentHolding;

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

	private void PointerInput_OnDragEnd(Vector2 obj)
	{
	}

	private void PointerInput_OnDrag(Vector2 obj)
	{
	}

	private void PointerInput_OnDragStart(Vector2 obj)
	{
	}

	private void PointerInput_OnHoldEnd(Vector2 obj)
	{
		currentHolding?.HoldEnd();
		currentHolding = null;
	}

	private void PointerInput_OnHoldStart(Vector2 obj)
	{
		Ray ray = Camera.main.ScreenPointToRay(obj);

		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, ClickableMask))
		{
			if (hit.collider.TryGetComponent<IClickable>(out var clickable))
				clickable.OnClick();
		}
		//var mousePos = GameInteractionHandler.GetMouseWorldPosition2D(obj);

		//RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
		//if (hit.collider != null)
		//{
		//	var hoverable = hit.collider.GetComponent<IHoverable>();
		//	if (hoverable != null)
		//	{
		//		//show info
		//		currentHolding = hoverable;
		//		currentHolding.HoldStart();
		//	}
		//}
	}

	private void PointerInput_OnClick(Vector2 obj)
	{
		var mousePos = GameInteractionHandler.GetMouseWorldPosition2D(obj);

		RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, Mathf.Infinity, ClickableMask);
		if (hit.collider != null)
		{
			var clickable = hit.collider.GetComponent<IClickable>();
			if (clickable != null)
			{
				clickable.OnClick();
			}
		}
	}
}
