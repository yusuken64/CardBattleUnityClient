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
    [Header("SpellCast Effects")]
    public List<SpellCastEffectWrapper> SpellCastEffects = new();

    public CustomSFX CustomSFX;

    public override CardBattleEngine.Card CreateCard()
	{
        var spellCard = new SpellCard(CardName, Cost);
        spellCard.SpriteID = ID;
        spellCard.SpellCastEffects.AddRange(SpellCastEffects.Select(x => x.Create()));
        spellCard.Description = string.Join(Environment.NewLine, SpellCastEffects.Select(ToDescription));
        spellCard.CustomSFX = CustomSFX;

        spellCard.ValidTargetSelector = ValidTargetSelector?.Create();
        spellCard.CastRestriction = CastRestriction?.Create();

        return spellCard;
    }
    public string ToDescription(SpellCastEffectWrapper spellCastEffect, int arg2)
    {
        if (!string.IsNullOrWhiteSpace(spellCastEffect.Description))
        {
            return spellCastEffect.Description;
        }

        string actions = string.Join(Environment.NewLine, spellCastEffect.GameActions.Select(ActionWrapperToDescription));
        string description = $"{actions}.";

        return description;
    }
}

[Serializable]
public class SpellCastEffectWrapper
{
    public string Description;

    [SerializeReference]
    public List<IGameActionWrapperBase> GameActions = new List<IGameActionWrapperBase>();

    [SerializeReference]
    public IAffectedEntitySelectorWrapperBase AffectedEntitySelectorWrapper;

    public SpellCastEffect Create()
	{
        return new SpellCastEffect()
        {
            GameActions = GameActions.Select(x => x.Create()).ToList(),
            AffectedEntitySelector = AffectedEntitySelectorWrapper?.Create(),
        };
	}
}