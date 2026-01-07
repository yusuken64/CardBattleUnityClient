using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SaveData
{
	[SerializeReference]
	public AppSaveData AppSaveData = new();
	[SerializeReference]
	public GameSaveData GameSaveData = new();
}

[Serializable]
public class AppSaveData
{
	public float MASTERVolume = 1f;
	public float MUSICVolume = 1f;
	public float GAMEVolume = 1f;
}

[Serializable]
public class GameSaveData
{
	public int CurrentDeckIndex = -1;
	[SerializeReference]
	public List<DeckSaveData> DeckSaveDatas = new();

	[SerializeReference]
	public StorySaveData StorySaveData = new();

	[SerializeReference]
	public AdventureSaveData AdventureSaveData = new();

	[SerializeReference]
	public DeckSaveData CombatDeck;
	public DeckSaveData CombatDeckEnemy;

	public CardCollection CardCollection = new();

	public int PackCount;
}

[Serializable]
public class CardCollection
{
	[SerializeField]
	private List<OwnedCardData> cards = new();

	private Dictionary<string, OwnedCardData> cache;

	private void InvalidateCache()
	{
		cache = null;
	}

	public Dictionary<string, OwnedCardData> Cards
	{
		get
		{
			if (cache == null)
				cache = cards.ToDictionary(c => c.CardID);
			return cache;
		}
	}

	public void Add(string cardId, int amount = 1)
	{
		if (amount <= 0)
			return;

		var owned = cards.FirstOrDefault(c => c.CardID == cardId);
		if (owned != null)
		{
			owned.Count += amount;
		}
		else
		{
			cards.Add(new OwnedCardData
			{
				CardID = cardId,
				Count = amount
			});
		}

		InvalidateCache();
	}

	public bool Remove(string cardId, int amount = 1)
	{
		var owned = cards.FirstOrDefault(c => c.CardID == cardId);
		if (owned == null)
			return false;

		owned.Count -= amount;

		if (owned.Count <= 0)
			cards.Remove(owned);

		InvalidateCache();
		return true;
	}

	public bool Has(string cardId, int amount = 1)
	{
		return Cards.TryGetValue(cardId, out var owned)
			&& owned.Count >= amount;
	}
}

[Serializable]
public class OwnedCardData
{
	public string CardID;
	public int Count;
}

[Serializable]
public class StorySaveData
{
	public List<string> CompletedLevels = new();
}

[Serializable]
public class AdventureSaveData
{
	public DeckSaveData CurrentDeck { get; internal set; }
}

[Serializable]
public class DeckSaveData
{
	public string Title;
	public string HeroCard;
	public List<string> CardIDs = new();

	internal Deck ToDeck()
	{
		var deck = new Deck();
		deck.Title = Title;
		deck.HeroCard = Common.Instance.CardManager.GetCardByName(HeroCard);
		deck.Cards = CardIDs.Select(x => Common.Instance.CardManager.GetCardByName(x)).ToList();

		return deck;
	}

	public static DeckSaveData FromDeck(Deck deck)
	{
		return new DeckSaveData()
		{
			Title = deck.Title,
			HeroCard = deck.HeroCard.CardName,
			CardIDs = deck.Cards.Select(x => x.CardName).ToList()
		};
	}
}