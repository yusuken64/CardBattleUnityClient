using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class WeaponValueWrapper : IValueProviderWrapperBase
{
    public CardBattleEngine.TeamRelationship Side;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.WeaponValue();
        instance.Side = this.Side;
        return instance;
    }
}