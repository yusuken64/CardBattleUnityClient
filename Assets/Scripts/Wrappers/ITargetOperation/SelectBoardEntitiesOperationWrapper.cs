using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class SelectBoardEntitiesOperationWrapper : ITargetOperationWrapperBase
{
    public CardBattleEngine.TargetSide Side;
    public CardBattleEngine.TargetGroup Group;

    public override CardBattleEngine.ITargetOperation Create()
    {
        var instance = new CardBattleEngine.SelectBoardEntitiesOperation();
        instance.Side = this.Side;
        instance.Group = this.Group;
        return instance;
    }
}