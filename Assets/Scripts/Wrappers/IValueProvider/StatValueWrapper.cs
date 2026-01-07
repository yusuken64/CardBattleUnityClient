using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class StatValueWrapper : IValueProviderWrapperBase
{
    public CardBattleEngine.Stat EntityStat;
    public CardBattleEngine.ContextProvider EntityContextProvider;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.StatValue();
        instance.EntityStat = this.EntityStat;
        instance.EntityContextProvider = this.EntityContextProvider;
        return instance;
    }
}