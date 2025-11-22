using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class GainManaActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Amount;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.GainManaAction();
        instance.Amount = this.Amount;
        instance.Canceled = this.Canceled;
        return instance;
    }
}