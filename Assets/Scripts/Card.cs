using CardBattleEngine;
using UnityEngine;

public class Card : MonoBehaviour, IDraggable
{
    public string CardName => this.name;

	public Vector2 TargetPosition { get; internal set; }
	public Quaternion TargetAngle { get; internal set; }
	public bool Moving { get; internal set; }
	public bool RequiresTarget;
    public CardType CardType;

    public GameObject VisualParent;

    public float moveSpeed;
    public float rotateSpeed;

    public void ResetVisuals()
	{
        VisualParent.transform.localScale = Vector3.one;
    }

    private void Update()
    {
        if (Moving && !Dragging)
        {
            // Move towards target in local space
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                TargetPosition,
                moveSpeed * Time.deltaTime
            );

            // Rotate in local space
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation,
                TargetAngle,
                rotateSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.localPosition, TargetPosition) < 0.05f &&
                Quaternion.Angle(transform.localRotation, TargetAngle) < 0.5f)
            {
                Moving = false;
            }
        }
    }

    public bool Dragging { get; set; }

    public bool CanStartDrag() => true;
}
