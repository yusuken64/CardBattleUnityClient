using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class HealthStatusSelectorWrapper : IValidTargetSelectorWrapperBase
{
    public CardBattleEngine.HealthStatus HealthStatus;

    public override CardBattleEngine.IValidTargetSelector Create()
    {
        var instance = new CardBattleEngine.HealthStatusSelector();
        instance.HealthStatus = this.HealthStatus;
        return instance;
    }
}