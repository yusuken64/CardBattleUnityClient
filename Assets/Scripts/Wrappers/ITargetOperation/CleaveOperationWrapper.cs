using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class CleaveOperationWrapper : ITargetOperationWrapperBase
{
    public System.Boolean IncludeCenter;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.CleaveOperation();
        instance.IncludeCenter = this.IncludeCenter;
        return instance;
    }
}