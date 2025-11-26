using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SummonedMinionConditionWrapper : ITriggerConditionWrapperBase
{
    public CardBattleEngine.TeamRelationship MinionToMinionRelationship;
    public CardBattleEngine.MinionTribe MinionTribe;
    public System.Boolean ExcludeSelf;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.SummonedMinionCondition();
        instance.MinionToMinionRelationship = this.MinionToMinionRelationship;
        instance.MinionTribe = this.MinionTribe;
        instance.ExcludeSelf = this.ExcludeSelf;
        return instance;
    }
}