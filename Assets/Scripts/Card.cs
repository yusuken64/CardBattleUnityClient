using CardBattleEngine;
using System;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour, IDraggable
{
    public string CardName => this.name;
    public CardBattleEngine.Card Data { get; private set; }

    #region Display
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI HealthText;

	#endregion

	#region Animation
	public Vector2 TargetPosition { get; internal set; }
	public Quaternion TargetAngle { get; internal set; }
	public bool Moving { get; internal set; }
	public bool RequiresTarget;
	public CardType CardType;

	public GameObject VisualParent;

	public float moveSpeed;
	public float rotateSpeed; 
	#endregion

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

	internal void Setup(CardBattleEngine.Card cardData)
	{
        this.Data = cardData;
        UpdateUI();
	}

	private void UpdateUI()
	{
        CostText.text = this.Data.ManaCost.ToString();

        if (this.Data is MinionCard minionCard)
        {
            AttackText.text = minionCard.Attack.ToString();
            HealthText.text = minionCard.Health.ToString();
        }
	}

	public bool Dragging { get; set; }

	public bool CanStartDrag() => true;
}
