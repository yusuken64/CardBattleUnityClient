using CardBattleEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Minion : MonoBehaviour, ITargetOrigin, ITargetable, IHoverable
{
    public CardBattleEngine.Minion Data { get; private set; }
	public MinionCard SummonedCard { get; set; }

    public Image CardImage;
	public int Attack;
    public int Health;
    public bool CanAttack;
    public bool HasDivineShield;
    public bool HasTaunt;
    public bool HasPoisonous;
    public bool HasSummoningSickness;
    public bool IsFrozen;
    public bool HasWindFury;
    public bool HasStealth;
    public bool HasLifeSteal;
    public bool HasReborn;

    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI HealthText;
    public GameObject AttackReadyIndicator;
    public GameObject ShieldIndicator;
    public GameObject TauntIndicator;
    public GameObject PoisonIndicator;
    public GameObject DeathRattleIndicator;
    public GameObject SleepIndicator;
    public GameObject FreezeIndicator;
    public GameObject WindFuryIndicator;
    public GameObject StealthIndicator;
    public GameObject LifeStealIndicator;
    public GameObject RebornIndicator;

	#region Animation
	public Vector2 TargetPosition { get; internal set; }
	public bool Moving { get; internal set; }

	public float moveSpeed;
	public float rotateSpeed; 
	#endregion

	// Update is called once per frame
	void Update()
    {
        if (Moving)
        {
            // Move towards target in local space
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                TargetPosition,
                moveSpeed * Time.deltaTime
            );

            //// Rotate in local space
            //transform.localRotation = Quaternion.RotateTowards(
            //    transform.localRotation,
            //    TargetAngle,
            //    rotateSpeed * Time.deltaTime
            //);

            if (Vector3.Distance(transform.localPosition, TargetPosition) < 0.05f
                //&& Quaternion.Angle(transform.localRotation, TargetAngle) < 0.5f
                )
            {
                Moving = false;
            }
        }
    }

	internal void SetTargetPosition(Vector2 vector2)
	{
        TargetPosition = vector2;
        Moving = true;
    }

    internal void Setup(CardBattleEngine.Minion minionData)
    {
        var cardDefinition = Common.Instance.CardManager.GetCardByName(minionData.Name);
        CardImage.sprite = cardDefinition.Sprite;

        this.Data = minionData;
        RefreshData(false);
	}

	private void UpdateUI()
	{
        AttackText.text = Attack.ToString();
        HealthText.text = Health.ToString();
        AttackReadyIndicator.gameObject.SetActive(CanAttack);

        TauntIndicator.gameObject.SetActive(HasTaunt);
        ShieldIndicator.gameObject.SetActive(HasDivineShield);
        PoisonIndicator.gameObject.SetActive(HasPoisonous);
        SleepIndicator.gameObject.SetActive(HasSummoningSickness && !CanAttack);
        FreezeIndicator.gameObject.SetActive(IsFrozen);
        WindFuryIndicator.gameObject.SetActive(HasWindFury);
        StealthIndicator.gameObject.SetActive(HasStealth);
        LifeStealIndicator.gameObject.SetActive(HasLifeSteal);
        RebornIndicator.gameObject.SetActive(HasReborn);
    }

    internal void RefreshData(bool activePlayerTurn)
    {
        if (Data == null) { return; }

        Attack = Data.Attack;
        Health = Data.Health;
        CanAttack = Data.CanAttack() && activePlayerTurn;
        HasDivineShield = Data.HasDivineShield;
        HasTaunt = Data.Taunt;
        HasPoisonous = Data.HasPoisonous;
        HasSummoningSickness = Data.HasSummoningSickness;
        IsFrozen = Data.IsFrozen;
        HasWindFury = Data.HasWindfury;
        HasStealth = Data.IsStealth;
        HasLifeSteal = Data.HasLifeSteal;
        HasReborn = Data.HasReborn;

        UpdateUI();
    }

	internal void SetupWithCard(CardBattleEngine.MinionCard data)
	{
        this.SummonedCard = data;
        Attack = data.Attack;
        Health = data.Health;

        var cardDefinition = Common.Instance.CardManager.GetCardByName(data.Name);
        CardImage.sprite = cardDefinition.Sprite;

        Attack = SummonedCard.Attack;
        Health = SummonedCard.Health;
        CanAttack = false;
        HasDivineShield = false;
        HasTaunt = false;
        HasPoisonous = false;
        HasSummoningSickness = false;
        IsFrozen = false;
        HasWindFury = false;
        HasStealth = false;

        UpdateUI();
    }

	public bool CanStartAiming()
	{
        return CanAttack;
	}

	public IGameEntity GetData()
	{
        return this.Data;
	}

	public CardBattleEngine.Player GetPlayer()
	{
        return this.Data.Owner;
	}

    //doesn't know if it's resolving an attack or battlecry target
    public void ResolveAim((IGameAction action, ActionContext context) current, GameObject gameObject)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        gameManager.ResolveAction(current.action, current.context);
    }

    public bool WillResolveSuccessfully(
        ITargetable target,
        GameObject pendingAimObject,
        out (IGameAction, ActionContext) current,
        Vector3 mousePos)
    {
        var gameManager = FindFirstObjectByType<GameManager>();

        if (SummonedCard != null)
        {
            var player = gameManager.GetPlayerFor(SummonedCard.Owner);
            var first = player.Hand.Cards.FirstOrDefault(x => x.Data == SummonedCard);
            player.Hand.Cards.Remove(first);
            PlayCardAction playCardAction = new()
            {
                Card = SummonedCard
            };
            ActionContext context = new()
            {
                SourcePlayer = SummonedCard.Owner,
                SourceCard = this.SummonedCard,
                Target = target.GetData(),
                PlayIndex = first._pendingIndex
            };
            current = (playCardAction, context);
        }
        else
        {
            AttackAction attackAction = new();
            ActionContext context = new()
            {
                SourcePlayer = Data.Owner,
                Source = Data,
                Target = target.GetData()
            };
            current = (attackAction, context);
        }

        return gameManager.CheckIsValid(current.Item1, current.Item2);
    }

	public AimIntent AimIntent { get; set; } = AimIntent.Attack;

    public GameObject DragObject => this.gameObject;
    public CardBattleEngine.Card GetDisplayCard()
    {
        return Data.OriginalCard;
    }
}
