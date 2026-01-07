using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SourceIsTargetConditionWrapper : ITriggerConditionWrapperBase
{


    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.SourceIsTargetCondition();

        return instance;
    }
}