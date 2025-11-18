using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
	public GameObject HeroPortrait;
	public Hand Hand;
	public Board Board;
	public GameObject DrawPile;
	public CardBattleEngine.Player Data { get; internal set; }

	public TextMeshProUGUI ManaText;

	internal void Clear()
	{
		foreach(var card in Hand.Cards)
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
}
