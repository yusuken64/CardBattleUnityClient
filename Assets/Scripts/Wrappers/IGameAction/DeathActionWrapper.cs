using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class DeathActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DeathAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}