using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AddStatModifierActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase AttackChange;
    [SerializeReference] public IValueProviderWrapperBase HealthChange;
    [SerializeReference] public IValueProviderWrapperBase CostChange;
    [SerializeReference] public ExpirationTriggerWrapper? ExpirationTrigger;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddStatModifierAction();
        instance.AttackChange = AttackChange?.Create();
        instance.HealthChange = HealthChange?.Create();
        instance.CostChange = CostChange?.Create();
        instance.ExpirationTrigger = ExpirationTrigger?.CreateEffect();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}