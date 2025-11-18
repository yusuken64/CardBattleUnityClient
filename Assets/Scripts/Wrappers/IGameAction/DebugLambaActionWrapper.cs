using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class DebugLambaActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DebugLambaAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}