using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class CastCardCostValueWrapper : IValueProviderWrapperBase
{


    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.CastCardCostValue();

        return instance;
    }
}