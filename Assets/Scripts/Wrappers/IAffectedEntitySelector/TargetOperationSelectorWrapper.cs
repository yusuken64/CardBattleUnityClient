using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class TargetOperationSelectorWrapper : IAffectedEntitySelectorWrapperBase
{
    [SerializeReference]
    public List<ITargetOperationWrapperBase> Operations;

    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.TargetOperationSelector();
        instance.Operations = this.Operations?.Select(x => x.Create()).ToList();
        return instance;
    }
}