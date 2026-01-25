using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class CombinedTargetSelectorWrapper : IValidTargetSelectorWrapperBase
{
    public IValidTargetSelectorWrapperBase Left;
    public CardBattleEngine.CombinationOperation Operation;
    public IValidTargetSelectorWrapperBase Right;

    public override CardBattleEngine.IValidTargetSelector Create()
    {
        var instance = new CardBattleEngine.CombinedTargetSelector();
        instance.Left = Left?.Create();
        instance.Operation = this.Operation;
        instance.Right = Right?.Create();
        return instance;
    }
}