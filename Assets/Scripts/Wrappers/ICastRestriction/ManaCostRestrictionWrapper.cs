using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ManaCostRestrictionWrapper : ICastRestrictionWrapperBase
{


    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.ManaCostRestriction();

        return instance;
    }
}