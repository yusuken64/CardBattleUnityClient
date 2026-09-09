using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SummonMinionActionWrapper : IGameActionWrapperBase
{
    public MinionCardDefinition Card;
    public System.Int32 IndexOffset;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SummonMinionAction();
        instance.Card = Card?.CreateCard() as MinionCard;
        instance.IndexOffset = this.IndexOffset;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}