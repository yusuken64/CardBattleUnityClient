using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class TriggerEffectActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.TriggerEffectAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}