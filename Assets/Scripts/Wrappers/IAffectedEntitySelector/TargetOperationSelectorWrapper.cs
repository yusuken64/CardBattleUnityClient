using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class TargetOperationSelectorWrapper : IAffectedEntitySelectorWrapperBase
{
    public System.Collections.Generic.List<ITargetOperationWrapperBase> Operations;

    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.TargetOperationSelector();
        instance.Operations = this.Operations?
            .Select(w => w?.Create())
            .ToList();
        return instance;
    }
}