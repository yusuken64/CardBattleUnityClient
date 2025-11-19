using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class GainManaActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Amount;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.Actions.GainManaAction();
        instance.Amount = this.Amount;
        instance.Canceled = this.Canceled;
        return instance;
    }
}