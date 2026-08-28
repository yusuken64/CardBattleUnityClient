using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class DeferredResolveActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IGameActionWrapperBase Action;
    [SerializeReference] public IAffectedEntitySelectorWrapperBase AffectedEntitySelector;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DeferredResolveAction();
        instance.Action = Action?.Create();
        instance.AffectedEntitySelector = AffectedEntitySelector?.Create();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}