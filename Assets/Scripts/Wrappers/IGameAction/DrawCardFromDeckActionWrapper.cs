using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class DrawCardFromDeckActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DrawCardFromDeckAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}