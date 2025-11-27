using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class OriginalSourceOwnerConditionWrapper : ITriggerConditionWrapperBase
{
    public CardBattleEngine.TeamRelationship TeamRelationship;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.OriginalSourceOwnerCondition();
        instance.TeamRelationship = this.TeamRelationship;
        return instance;
    }
}