using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AcquireWeaponActionWrapper : IGameActionWrapperBase
{
    public WeaponCardDefinition Weapon;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AcquireWeaponAction();
        instance.Weapon = (Weapon?.CreateCard() as WeaponCard)?.CreateWeapon();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}