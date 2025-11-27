using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class HealedAmoutValueWrapper : IValueProviderWrapperBase
{


    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.HealedAmoutValue();

        return instance;
    }
}