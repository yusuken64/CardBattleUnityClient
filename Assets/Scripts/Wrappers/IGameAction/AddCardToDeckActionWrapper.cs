using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AddCardToDeckActionWrapper : IGameActionWrapperBase
{
    public CardDefinition Card;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddCardToDeckAction();
        instance.Card = Card?.CreateCard();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}