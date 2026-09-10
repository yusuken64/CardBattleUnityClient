using System.Linq;

public static class DeckNetworkExtensions
{
	public static DecklistRequest ToDecklistRequest(this Deck deck, string playerName)
	{
		var request = new DecklistRequest { PlayerName = playerName };

		var grouped = deck.Cards
			.GroupBy(card => card.CardName)
			.Select(group => new { Definition = group.First(), Count = group.Count() });

		foreach (var entry in grouped)
		{
			var cardCount = new CardCount { CardId = entry.Definition.CardName, Count = entry.Count };

			if (entry.Definition is MinionCardDefinition)
			{
				request.Minions.Add(cardCount);
			}
			else if (entry.Definition is SpellCardDefinition)
			{
				request.Spells.Add(cardCount);
			}
			else if (entry.Definition is WeaponCardDefinition)
			{
				request.Weapons.Add(cardCount);
			}
		}

		return request;
	}
}
