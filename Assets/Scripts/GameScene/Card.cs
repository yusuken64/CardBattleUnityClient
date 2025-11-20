using CardBattleEngine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour, IDraggable
{
    public string CardName => this.name;
    public Image CardImage;
    public CardBattleEngine.Card Data { get; private set; }

    #region Display
    public int Cost;
    public int Attack;
    public int Health;
    public bool CanPlay;
    public TextMeshProUGUI CostText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI HealthText;
    public GameObject CanPlayIndicator;

    #endregion

    #region Animation
    public Vector2 TargetPosition { get; internal set; }
    public Quaternion TargetAngle { get; internal set; }
    public bool Moving { get; internal set; }

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
        var cardManager = FindFirstObjectByType<CardManager>();
        var cardDefinition = cardManager.GetCardByName(cardData.Name);
        this.CardImage.sprite = cardDefinition.Sprite;

        this.Data = cardData;
        RefreshData(false);
    }

    private void UpdateUI()
    {
        CostText.text = Cost.ToString();
        CanPlayIndicator.gameObject.SetActive(CanPlay);
        if (AttackText != null) AttackText.text = Attack.ToString();
        if (HealthText != null) HealthText.text = Health.ToString();
    }

    internal void RefreshData(bool activePlayerTurn)
    {
        Cost = this.Data.ManaCost;
        CanPlay = this.Data.Owner.Mana >= Cost && activePlayerTurn;

        if (this.Data is MinionCard minionCard)
        {
            Attack = minionCard.Attack;
            Health = minionCard.Health;
        }
        UpdateUI();
    }

    public bool Dragging { get; set; }

	public bool CanStartDrag() => true;
    public bool RequiresTarget()
    {
        if (this.Data is MinionCard minionCard)
        {
        }
        else if (this.Data is SpellCard spellCard)
        {
            return true;
        }

        return false;
    }
}
