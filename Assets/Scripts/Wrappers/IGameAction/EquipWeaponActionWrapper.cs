using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class EquipWeaponActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Weapon Weapon;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.EquipWeaponAction();
        instance.Weapon = this.Weapon;
        instance.Canceled = this.Canceled;
        return instance;
    }
}