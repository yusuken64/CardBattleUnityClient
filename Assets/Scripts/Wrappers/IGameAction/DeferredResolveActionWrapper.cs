using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class DeferredResolveActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.TargetOperationSelector AffectedEntitySelector;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DeferredResolveAction();
        instance.AffectedEntitySelector = this.AffectedEntitySelector;
        instance.Canceled = this.Canceled;
        return instance;
    }
}