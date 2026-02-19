using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CardCounter : MonoBehaviour
{
	public List<DeckDefinition> Decks;

	Dictionary<string, int> CardCount;

	private void Start()
	{
		CountCards();
	}

	public void CountCards()
	{
		CardCount = new();

		var allCards = Common.Instance.CardManager.Cards.Where(x => x.Collectable);
		foreach(var card in allCards)
		{
			CardCount[card.CardName] = 0;
		}

		foreach(var deck in Decks)
		{
			foreach(var card in deck.Cards)
			{
				if (CardCount.ContainsKey(card.CardName))
					CardCount[card.CardName]++;
				else
					CardCount[card.CardName] = 1;

			}
		}

		var ordered = CardCount.OrderByDescending(kvp => kvp.Value)
							 .ThenBy(kvp => kvp.Key); // optional alphabetical tiebreak

		var lines = new List<string> { "CardName,Count" };
		lines.AddRange(ordered.Select(kvp => $"{kvp.Key},{kvp.Value}"));

		// File path
		string filePath = Path.Combine(Application.persistentDataPath, "CardCounts.csv");

		// Write file
		File.WriteAllLines(filePath, lines);

		Debug.Log($"Card counts exported to CSV: {filePath}");
	}
}
