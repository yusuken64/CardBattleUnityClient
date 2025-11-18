using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class EndTurnActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.EndTurnAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}