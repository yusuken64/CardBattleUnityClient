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

    [Header("Triggered Effects")]
    public List<TriggeredEffectWrapper> TriggeredEffects = new List<TriggeredEffectWrapper>();

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
            HasTaunt = this.HasTaunt
        };

        return card;
    }
}

[Serializable]
public class TriggeredEffectWrapper
{
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
            GameActions = GameActions.Select(x => x.Create()).ToList(),
            Condition = Condition.Create(),
            AffectedEntitySelector = AffectedEntitySelectorWrapper.Create()
        };

        return effect;
    }
}