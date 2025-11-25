using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ContextSelectorWrapper : IAffectedEntitySelectorWrapperBase
{


    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.ContextSelector();

        return instance;
    }
}