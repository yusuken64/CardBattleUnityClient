using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class RebornActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RebornAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}