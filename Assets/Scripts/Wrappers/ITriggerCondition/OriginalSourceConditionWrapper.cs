using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class OriginalSourceConditionWrapper : ITriggerConditionWrapperBase
{


    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.OriginalSourceCondition();

        return instance;
    }
}