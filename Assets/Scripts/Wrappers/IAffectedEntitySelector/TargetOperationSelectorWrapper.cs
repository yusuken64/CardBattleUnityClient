using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class TargetOperationSelectorWrapper : IAffectedEntitySelectorWrapperBase
{
    //public System.Collections.Generic.List<CardBattleEngine.ITargetOperation> Operations;

    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.TargetOperationSelector();
        //instance.Operations = this.Operations;
        return instance;
    }
}