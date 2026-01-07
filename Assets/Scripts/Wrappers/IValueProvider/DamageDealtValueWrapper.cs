using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class DamageDealtValueWrapper : IValueProviderWrapperBase
{


    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.DamageDealtValue();

        return instance;
    }
}