using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewSpellCard",
    menuName = "Game/Cards/SpellCard Definition"
)]
public class SpellCardDefinition : CardDefinition
{
    public TargetingType TargetingType;

    [Header("SpellCast Effects")]
    public List<SpellCastEffectWrapper> SpellCastEffects = new();

	internal override CardBattleEngine.Card CreateCard()
	{
        var spellCard = new SpellCard(CardName, Cost);
        spellCard.SpellCastEffects.AddRange(SpellCastEffects.Select(x => x.Create()));

        return spellCard;
	}
}

[Serializable]
public class SpellCastEffectWrapper
{
    [SerializeReference]
    public List<IGameActionWrapperBase> GameActions = new List<IGameActionWrapperBase>();

    [SerializeReference]
    public IAffectedEntitySelectorWrapperBase AffectedEntitySelectorWrapper;

    public SpellCastEffect Create()
	{
        return new SpellCastEffect()
        {
            GameActions = GameActions.Select(x => x.Create()).ToList(),
            AffectedEntitySelector = AffectedEntitySelectorWrapper?.Create()
        };
	}
}