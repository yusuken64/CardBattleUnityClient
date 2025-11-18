using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class RefillManaActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RefillManaAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}