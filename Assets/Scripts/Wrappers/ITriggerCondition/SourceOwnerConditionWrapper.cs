using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SourceOwnerConditionWrapper : ITriggerConditionWrapperBase
{
    public CardBattleEngine.TeamRelationship TeamRelationship;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.SourceOwnerCondition();
        instance.TeamRelationship = this.TeamRelationship;
        return instance;
    }
}