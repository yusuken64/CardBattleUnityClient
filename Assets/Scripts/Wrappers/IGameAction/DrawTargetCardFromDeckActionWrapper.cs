using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class DrawTargetCardFromDeckActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DrawTargetCardFromDeckAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}