using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class CombinedRestrictionWrapper : ICastRestrictionWrapperBase
{
    [SerializeReference] public ICastRestrictionWrapperBase Left;
    public CardBattleEngine.CombinationOperation Operation;
    [SerializeReference] public ICastRestrictionWrapperBase Right;

    public override CardBattleEngine.ICastRestriction Create()
    {
        var instance = new CardBattleEngine.CombinedRestriction();
        instance.Left = Left?.Create();
        instance.Operation = this.Operation;
        instance.Right = Right?.Create();
        return instance;
    }
}