using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SpellOwnerConditionWrapper : ITriggerConditionWrapperBase
{
    public CardBattleEngine.TargetSide TargetSide;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.SpellOwnerCondition();
        instance.TargetSide = this.TargetSide;
        return instance;
    }
}