using System;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour, ITargetable
{
	public GameObject HeroPortrait;
	public Hand Hand;
	public Board Board;
	public GameObject DrawPile;
	public CardBattleEngine.Player Data { get; internal set; }

	public int Health;

	public TextMeshProUGUI ManaText;
	public TextMeshProUGUI HealthText;

	internal void Clear()
	{
		foreach (var card in Hand.Cards)
		{
			Destroy(card.gameObject);
		}

		foreach (var minion in Board.Minions)
		{
			Destroy(minion.gameObject);
		}

		Hand.Cards.Clear();
		Board.Minions.Clear();
	}

	internal void RefreshData(bool activePlayerTurn)
	{
		Health = Data.Health;

		foreach(var minion in Board.Minions)
		{
			minion.RefreshData(activePlayerTurn);
		}

		foreach (var card in Hand.Cards)
		{
			card.RefreshData(activePlayerTurn);
		}

		UpdateUI();
	}

	private void UpdateUI()
	{
		HealthText.text = Health.ToString();
	}

	public CardBattleEngine.IGameEntity GetData()
	{
		return this.Data;
	}

	public AimIntent AimIntent { get; set; } = AimIntent.Attack; 
}
