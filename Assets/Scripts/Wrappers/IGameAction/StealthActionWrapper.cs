using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class StealthActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.StealthAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}