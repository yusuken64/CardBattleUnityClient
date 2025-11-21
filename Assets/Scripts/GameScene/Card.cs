using CardBattleEngine;
using System.Collections.Generic;
using System.Linq;
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

    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public GameObject TribeObject;
    public TextMeshProUGUI TribeText;

    #endregion

    #region Animation
    public Vector2 TargetPosition { get; internal set; }
    public Quaternion TargetAngle { get; internal set; }
    public bool Moving { get; internal set; }

    public CardType CardType => this.Data.Type;

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
        NameText.text = Data.Name;
        string description = "";
        if (this.Data is MinionCard minionCard)
        {
            TribeObject.gameObject.SetActive(false);
            //TribeObject.gameObject.SetActive(minionCard.MinionTribes.Any(x => x != MinionTribe.None));
            //TribeText.text = minionCard.MinionTribes[0].ToString();

            var keyWords = new List<string>();
            if (minionCard.HasCharge) { keyWords.Add("Charge"); }
            if (minionCard.HasTaunt) { keyWords.Add("Taunt"); }
            if (minionCard.HasDivineShield) { keyWords.Add("Divine Shield"); }
            if (minionCard.HasPoisonous) { keyWords.Add("Poison"); }
            if (minionCard.IsStealth) { keyWords.Add("Stealth"); }
            description = string.Join(",", keyWords);
        }
		else
		{
            TribeObject.gameObject.SetActive(false);
        }
        //description = Data.CardText;
        DescriptionText.text = description;

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
            if (spellCard.TargetingType == TargetingType.None)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

	public IGameEntity GetData()
	{
        return this.Data as IGameEntity;
	}
}
