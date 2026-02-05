using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewDeck",
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

    [ContextMenu("SortDeck")]
    public void SortDeck()
	{
        SortByManaThenName();
    }

    public void SortCards(Comparison<CardDefinition> comparison)
    {
        if (Cards == null || Cards.Count <= 1)
            return;

        Cards.Sort(comparison);
    }

    public void SortByManaThenName()
    {
        SortCards((a, b) =>
        {
            int manaCompare = a.Cost.CompareTo(b.Cost);
            if (manaCompare != 0)
                return manaCompare;

            return string.Compare(a.CardName, b.CardName, StringComparison.Ordinal);
        });
    }

#if UNITY_EDITOR
    [ContextMenu("Randomize Cards")]
    public void RandomizeCards()
    {
    }
#endif
}
