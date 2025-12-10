using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AddStatModifierActionWrapper : IGameActionWrapperBase
{
    public System.Int32 AttackChange;
    public System.Int32 HealthChange;
    public System.Int32 CostChange;
    [SerializeReference] public ExpirationTriggerWrapper? ExpirationTrigger;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddStatModifierAction();
        instance.AttackChange = this.AttackChange;
        instance.HealthChange = this.HealthChange;
        instance.CostChange = this.CostChange;
        instance.ExpirationTrigger = ExpirationTrigger?.CreateEffect();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}