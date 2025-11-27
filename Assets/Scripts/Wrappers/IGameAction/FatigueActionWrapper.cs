using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class FatigueActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.FatigueAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}