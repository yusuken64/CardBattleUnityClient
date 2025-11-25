using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ContextOperationWrapper : ITargetOperationWrapperBase
{
    public System.Boolean IncludeTarget;
    public System.Boolean IncludeSource;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.ContextOperation();
        instance.IncludeTarget = this.IncludeTarget;
        instance.IncludeSource = this.IncludeSource;
        return instance;
    }
}