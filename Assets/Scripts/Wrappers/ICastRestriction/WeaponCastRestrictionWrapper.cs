using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class WeaponCastRestrictionWrapper : ICastRestrictionWrapperBase
{
    public CardBattleEngine.TeamRelationship TeamRelationship;

    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.WeaponCastRestriction();
        instance.TeamRelationship = this.TeamRelationship;
        return instance;
    }
}