using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class TargetTypeConditionWrapper : ITriggerConditionWrapperBase
{
    public CardBattleEngine.EntityType EntityTypes;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.TargetTypeCondition();
        instance.EntityTypes = this.EntityTypes;
        return instance;
    }
}