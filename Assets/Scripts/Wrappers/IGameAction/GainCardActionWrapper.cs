using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class GainCardActionWrapper : IGameActionWrapperBase
{
    public CardDefinition Card;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.GainCardAction();
        instance.Card = Card?.CreateCard();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}