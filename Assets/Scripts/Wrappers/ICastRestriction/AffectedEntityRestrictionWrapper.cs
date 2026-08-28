using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AffectedEntityRestrictionWrapper : ICastRestrictionWrapperBase
{
    [SerializeReference] public IAffectedEntitySelectorWrapperBase IAffectedEntitySelector;

    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.AffectedEntityRestriction();
        instance.IAffectedEntitySelector = IAffectedEntitySelector?.Create();
        return instance;
    }
}