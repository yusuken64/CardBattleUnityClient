using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ArmorRestrictionWrapper : ICastRestrictionWrapperBase
{


    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.ArmorRestriction();

        return instance;
    }
}