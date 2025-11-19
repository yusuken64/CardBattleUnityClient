using CardBattleEngine;
using System;
using TMPro;
using UnityEngine;

public class Minion : MonoBehaviour
{
    public CardBattleEngine.Minion Data { get; private set; }
	public MinionCard SummonedCard { get; private set; }

	public int Attack;
    public int Health;
    public bool CanAttack;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI HealthText;
    public GameObject AttackReadyIndicator;

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
        this.Data = minionData;
        RefreshData(false);
	}

	private void UpdateUI()
	{
        AttackText.text = Attack.ToString();
        HealthText.text = Health.ToString();
        AttackReadyIndicator.gameObject.SetActive(CanAttack);
    }

    internal void RefreshData(bool activePlayerTurn)
    {
        if (Data == null) { return; }

        Attack = Data.Attack;
        Health = Data.Health;
        CanAttack = Data.CanAttack() && activePlayerTurn;

        UpdateUI();
    }

	internal void SetupWithCard(CardBattleEngine.MinionCard data)
	{
        this.SummonedCard = data;
        Attack = data.Attack;
        Health = data.Health;
	}
}
