using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class DestroyWeaponActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DestroyWeaponAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}