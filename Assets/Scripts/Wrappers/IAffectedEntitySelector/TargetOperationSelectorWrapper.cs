using System;
using UnityEngine;
using CardBattleEngine;
using System.Linq;

[Serializable]
public class TargetOperationSelectorWrapper : IAffectedEntitySelectorWrapperBase
{
    [SerializeReference]
    public System.Collections.Generic.List<ITargetOperationWrapperBase> Operations;

    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.TargetOperationSelector();
        instance.Operations = this.Operations.Select(x => x.Create()).ToList();
        return instance;
    }
}