using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ConstantValueWrapper : IValueProviderWrapperBase
{
    public System.Int32 Number;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.ConstantValue();
        instance.Number = this.Number;
        return instance;
    }
}