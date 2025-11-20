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
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddStatModifierAction();
        instance.AttackChange = this.AttackChange;
        instance.HealthChange = this.HealthChange;
        instance.Canceled = this.Canceled;
        return instance;
    }
}