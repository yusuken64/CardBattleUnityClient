using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class SummonMinionActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SummonMinionAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}