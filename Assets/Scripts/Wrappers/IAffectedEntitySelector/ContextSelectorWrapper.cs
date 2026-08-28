using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ContextSelectorWrapper : IAffectedEntitySelectorWrapperBase
{
    public System.Boolean IncludeTarget;
    public System.Boolean IncludeSource;
    public System.Boolean IncludeSourcePlayer;
    public System.Boolean IncludeTargetOwner;
    public System.Boolean IncludeSummonedMinion;
    public CardBattleEngine.TargetResolutionTiming ResolutionTiming;

    public override CardBattleEngine.IAffectedEntitySelector Create()
    {
        var instance = new CardBattleEngine.ContextSelector();
        instance.IncludeTarget = this.IncludeTarget;
        instance.IncludeSource = this.IncludeSource;
        instance.IncludeSourcePlayer = this.IncludeSourcePlayer;
        instance.IncludeTargetOwner = this.IncludeTargetOwner;
        instance.IncludeSummonedMinion = this.IncludeSummonedMinion;
        instance.ResolutionTiming = this.ResolutionTiming;
        return instance;
    }
}