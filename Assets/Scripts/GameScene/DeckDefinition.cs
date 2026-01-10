using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Game/Cards/Deck Definition"
)]
public class DeckDefinition : ScriptableObject
{
    public string Title;
    public CardDefinition HeroCard;
    //public HeroPowerDefinition HeroPower;
    public List<CardDefinition> Cards;

	internal Deck ToDeck()
    {
        return new Deck()
        {
            Title = Title,
            HeroCard = HeroCard,
            Cards = Cards,
        };
    }

    public DeckSaveData ToDeckData()
	{
        return new DeckSaveData()
		{
            Title = Title,
            HeroCard = HeroCard.ID,
            CardIDs = Cards.Select(x => x.ID).ToList()
		};
	}
}
