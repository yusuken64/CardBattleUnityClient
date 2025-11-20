using CardBattleEngine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Minion : MonoBehaviour, ITargetOrigin, ITargetable
{
    public CardBattleEngine.Minion Data { get; private set; }
	public MinionCard SummonedCard { get; private set; }

    public Image CardImage;
	public int Attack;
    public int Health;
    public bool CanAttack;
    public bool HasDivineShield;
    public bool HasTaunt;
    public bool HasPoisonous;
    public bool HasSummoningSickness;

    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI HealthText;
    public GameObject AttackReadyIndicator;
    public GameObject ShieldIndicator;
    public GameObject TauntIndicator;
    public GameObject PoisonIndicator;
    public GameObject DeathRattleIndicator;
    public GameObject SleepIndicator;

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
        var cardDefinition = FindFirstObjectByType<CardManager>().GetCardByName(minionData.Name);
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

        UpdateUI();
    }

	internal void SetupWithCard(CardBattleEngine.MinionCard data)
	{
        this.SummonedCard = data;
        Attack = data.Attack;
        Health = data.Health;
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

	public AimIntent AimIntent { get; set; } = AimIntent.Attack;
}
