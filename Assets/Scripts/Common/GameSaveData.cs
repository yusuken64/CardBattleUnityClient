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
	[SerializeReference]
	public List<DeckSaveData> DeckSaveDatas = new();
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

	public DeckSaveData FromDeck(Deck deck)
	{
		return new DeckSaveData()
		{
			Title = deck.Title,
			HeroCard = deck.HeroCard.CardName,
			CardIDs = deck.Cards.Select(x => x.CardName).ToList()
		};
	}
}