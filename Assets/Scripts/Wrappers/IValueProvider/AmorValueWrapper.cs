using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AmorValueWrapper : IValueProviderWrapperBase
{


    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.AmorValue();

        return instance;
    }
}