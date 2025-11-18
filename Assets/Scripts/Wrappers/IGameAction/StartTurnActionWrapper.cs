using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class StartTurnActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.StartTurnAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}