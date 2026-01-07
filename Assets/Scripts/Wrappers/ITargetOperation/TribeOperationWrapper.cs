using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class TribeOperationWrapper : ITargetOperationWrapperBase
{
    public CardBattleEngine.MinionTribe Tribe;
    public System.Boolean ExcludeSelf;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.TribeOperation();
        instance.Tribe = this.Tribe;
        instance.ExcludeSelf = this.ExcludeSelf;
        return instance;
    }
}