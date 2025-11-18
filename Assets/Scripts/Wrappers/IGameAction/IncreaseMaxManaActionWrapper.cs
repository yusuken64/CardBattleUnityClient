using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class IncreaseMaxManaActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Amount;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.IncreaseMaxManaAction();
        instance.Amount = this.Amount;
        instance.Canceled = this.Canceled;
        return instance;
    }
}