using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class EquipWeaponActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Weapon Weapon;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        return null;
        //var instance = new CardBattleEngine.EquipWeaponAction();
        //instance.Weapon = this.Weapon;
        //instance.Canceled = this.Canceled;
        //return instance;
    }
}