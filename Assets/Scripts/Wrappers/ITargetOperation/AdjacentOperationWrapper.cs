using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AdjacentOperationWrapper : ITargetOperationWrapperBase
{
    public System.Boolean UsePlayIndexInstead;
    public System.Boolean IncludeCenter;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.AdjacentOperation();
        instance.UsePlayIndexInstead = this.UsePlayIndexInstead;
        instance.IncludeCenter = this.IncludeCenter;
        return instance;
    }
}