using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class FreezeActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.FreezeAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}