using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class AcquireWeaponActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Weapon Weapon;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AcquireWeaponAction();
        instance.Weapon = this.Weapon;
        instance.Canceled = this.Canceled;
        return instance;
    }
}