using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class StatSelectorWrapper : IValidTargetSelectorWrapperBase
{
    public CardBattleEngine.Stat Stat;
    public CardBattleEngine.Comparison Comparison;
    public System.Int32 Value;

    public override CardBattleEngine.IValidTargetSelector Create()
    {
        var instance = new CardBattleEngine.StatSelector();
        instance.Stat = this.Stat;
        instance.Comparison = this.Comparison;
        instance.Value = this.Value;
        return instance;
    }
}