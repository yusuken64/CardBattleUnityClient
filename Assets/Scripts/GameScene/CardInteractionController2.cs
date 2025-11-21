using UnityEngine;
using UnityEngine.UI;

public class CardInteractionController2 : MonoBehaviour
{
    public PointerInput PointerInput;
	public LayerMask ClickableMask;
	private IDraggable currentDraggable;
	private ITargetOrigin currentAimable;

	public LineRenderer LineRenderer;
	public Image ArrowHead;
	public int CurveResolution = 20;
	public float CurveStrength = 2f;
	private Vector3 _aimStartPosition;
	private GameObject pendingAimObject;
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

			if (MouseOverBoard(mousePos).collider != null)
			{
				currentDraggable.Resolve(mousePos);
			}
			else
			{
				currentDraggable.CancelDrag();
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
			}

			if (target != null &&
				currentAimable.WillResolveSuccessfully(target, pendingAimObject, out var current))
			{
				currentAimable.ResolveAim(current);
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
		pendingDraggable.CancelAim();
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
