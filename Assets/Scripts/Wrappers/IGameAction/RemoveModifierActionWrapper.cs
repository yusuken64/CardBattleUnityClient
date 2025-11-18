using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class RemoveModifierActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RemoveModifierAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}