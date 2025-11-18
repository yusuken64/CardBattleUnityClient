using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class AttackActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AttackAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}