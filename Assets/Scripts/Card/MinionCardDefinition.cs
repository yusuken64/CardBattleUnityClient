using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewMinionCard",
    menuName = "Game/Cards/MinionCard Definition"
)]
public class MinionCardDefinition : CardDefinition
{
    public int Attack = 1;
    public int Health = 1;
    public List<MinionTribe> MinionTribes = new List<MinionTribe>();
    public bool IsStealth;
    public bool HasCharge;
    public bool HasDivineShield;
    public bool HasTaunt;
    public bool HasPoisonous;
    public bool HasWindFury;
    public bool HasLifeSteal;
    public bool HasReborn;

    public List<TriggeredEffectWrapper> MinionTriggeredEffects = new List<TriggeredEffectWrapper>();

    // Creates a runtime MinionCard from this definition
    internal override CardBattleEngine.Card CreateCard()
    {
        MinionCard card = new MinionCard(CardName, Cost, Attack, Health)
        {
            MinionTribes = new List<MinionTribe>(MinionTribes),
            IsStealth = this.IsStealth,
            HasCharge = this.HasCharge,
            HasDivineShield = this.HasDivineShield,
            HasPoisonous = this.HasPoisonous,
            HasTaunt = this.HasTaunt,
            HasWindfury = this.HasWindFury,
            HasReborn = this.HasReborn,
            HasLifeSteal = this.HasLifeSteal,
        };
        card.TriggeredEffects.AddRange(TriggeredEffects.Select(x => x.CreateEffect()));
        card.MinionTriggeredEffects.AddRange(MinionTriggeredEffects.Select(x => x.CreateEffect()));

        var triggeredEffects = new List<TriggeredEffectWrapper>();
        triggeredEffects.AddRange(TriggeredEffects);
        triggeredEffects.AddRange(MinionTriggeredEffects);
        card.Description = string.Join(Environment.NewLine, triggeredEffects.Select(ToDescription));

        return card;
    }
}

[Serializable]
public class TriggeredEffectWrapper
{
    public string Description;
    public EffectTrigger EffectTrigger;
    public EffectTiming EffectTiming;
    public TargetingType TargetType;

    [SerializeReference]
    public List<IGameActionWrapperBase> GameActions = new List<IGameActionWrapperBase>();

    [SerializeReference]
    public ITriggerConditionWrapperBase Condition;

    [SerializeReference]
    public IAffectedEntitySelectorWrapperBase AffectedEntitySelectorWrapper;

    // The real TriggeredEffect instance is generated at runtime
    public TriggeredEffect CreateEffect()
    {
        TriggeredEffect effect = new TriggeredEffect()
        {
            EffectTrigger = this.EffectTrigger,
            EffectTiming = this.EffectTiming,
            TargetType = this.TargetType,
            GameActions = GameActions?.Select(x => x.Create()).ToList(),
            Condition = Condition?.Create(),
            AffectedEntitySelector = AffectedEntitySelectorWrapper?.Create()
        };

        return effect;
    }
}