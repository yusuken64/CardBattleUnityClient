using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ValidTargetRestrictionWrapper : ICastRestrictionWrapperBase
{


    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.ValidTargetRestriction();

        return instance;
    }
}