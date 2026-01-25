using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class EntityTypeSelectorWrapper : IValidTargetSelectorWrapperBase
{
    public CardBattleEngine.EntityType EntityTypes;
    public CardBattleEngine.TeamRelationship TeamRelationship;

    public override CardBattleEngine.IValidTargetSelector Create()
    {
        var instance = new CardBattleEngine.EntityTypeSelector();
        instance.EntityTypes = this.EntityTypes;
        instance.TeamRelationship = this.TeamRelationship;
        return instance;
    }
}