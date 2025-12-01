using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class NegativeValueWrapper : IValueProviderWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase OriginalValue;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.NegativeValue();
        instance.OriginalValue = OriginalValue?.Create();
        return instance;
    }
}