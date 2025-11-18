using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class SummonedMinionTribeConditionWrapper : ITriggerConditionWrapperBase
{
    public System.Boolean ExcludeSelf;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.SummonedMinionTribeCondition();
        instance.ExcludeSelf = this.ExcludeSelf;
        return instance;
    }
}