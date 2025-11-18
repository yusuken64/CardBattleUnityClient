using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class DamageActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Damage;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DamageAction();
        instance.Damage = this.Damage;
        instance.Canceled = this.Canceled;
        return instance;
    }
}