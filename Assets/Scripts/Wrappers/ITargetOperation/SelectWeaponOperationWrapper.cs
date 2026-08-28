using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SelectWeaponOperationWrapper : ITargetOperationWrapperBase
{
    public CardBattleEngine.TeamRelationship Side;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.SelectWeaponOperation();
        instance.Side = this.Side;
        return instance;
    }
}