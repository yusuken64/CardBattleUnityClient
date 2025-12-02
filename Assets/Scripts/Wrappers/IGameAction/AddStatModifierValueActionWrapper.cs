using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AddStatModifierValueActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase AttackChange;
    [SerializeReference] public IValueProviderWrapperBase HealthChange;
    [SerializeReference] public IValueProviderWrapperBase CostChange;
    [SerializeReference] public ExpirationTriggerWrapper? ExpirationTrigger;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddStatModifierValueAction();
        instance.AttackChange = AttackChange?.Create();
        instance.HealthChange = HealthChange?.Create();
        instance.CostChange = CostChange?.Create();
        instance.ExpirationTrigger = ExpirationTrigger?.CreateEffect();
        instance.Canceled = this.Canceled;
        return instance;
    }
}