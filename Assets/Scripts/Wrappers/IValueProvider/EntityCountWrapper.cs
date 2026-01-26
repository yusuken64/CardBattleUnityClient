using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class EntityCountWrapper : IValueProviderWrapperBase
{
    [SerializeReference] public IAffectedEntitySelectorWrapperBase AffectedEntitySelector;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.EntityCount();
        instance.AffectedEntitySelector = AffectedEntitySelector?.Create();
        return instance;
    }
}