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
    public HeroPowerDefinition HeroPower;
    public List<CardDefinition> Cards;

    internal CardBattleEngine.HeroPower CreateHeroPowerFromHeroCard()
    {
        if (HeroCard is MinionCardDefinition mimionCard)
        {
            if (mimionCard.TriggeredEffects != null &&
                mimionCard.TriggeredEffects.Any())
            {
                TriggeredEffectWrapper triggeredEffectWrapper = mimionCard.TriggeredEffects[0];
                return new CardBattleEngine.HeroPower()
                {
                    Name = $"Invoke {HeroCard.CardName}",
                    TargetingType = triggeredEffectWrapper.TargetType,
                    AffectedEntitySelector = mimionCard.TriggeredEffects[0].AffectedEntitySelectorWrapper?.Create(),
                    GameActions = mimionCard.TriggeredEffects[0].GameActions.Select(x => x.Create()).ToList(),
                    ManaCost = HeroCard.Cost,
                    UsedThisTurn = false
                };
            }
        }
        return null; //TODO allow other types or restrict to minion cards
    }

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
            CardIDs = Cards.Select(x => x.CardName).ToList()
		};
	}
}
