using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class RandomOperationWrapper : ITargetOperationWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase Count;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.RandomOperation();
        instance.Count = Count?.Create();
        return instance;
    }
}