using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class StatValueWrapper : IValueProviderWrapperBase
{
    public CardBattleEngine.ValueProviders.Stat EntityStat;
    public CardBattleEngine.ValueProviders.ContextProvider EntityContextProvider;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.ValueProviders.StatValue();
        instance.EntityStat = this.EntityStat;
        instance.EntityContextProvider = this.EntityContextProvider;
        return instance;
    }
}