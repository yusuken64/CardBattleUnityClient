using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class CastSpellActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.CastSpellAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}