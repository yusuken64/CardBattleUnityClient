using System;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
	public GameObject HeroPortrait;
	public Hand Hand;
	public Board Board;
	public GameObject DrawPile;
	public CardBattleEngine.Player Data { get; internal set; }

	public int Health { get => health; set => health = value; }

	public TextMeshProUGUI ManaText;
	private int health;

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
	}
}
