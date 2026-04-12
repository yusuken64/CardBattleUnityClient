using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class CombinedConditionWrapper : ITriggerConditionWrapperBase
{
    [SerializeReference] public ITriggerConditionWrapperBase Left;
    public CardBattleEngine.CombinationOperation Operation;
    [SerializeReference] public ITriggerConditionWrapperBase Right;

    public override CardBattleEngine.ITriggerCondition Create()
    {
        var instance = new CardBattleEngine.CombinedCondition();
        instance.Left = Left?.Create();
        instance.Operation = this.Operation;
        instance.Right = Right?.Create();
        return instance;
    }
}