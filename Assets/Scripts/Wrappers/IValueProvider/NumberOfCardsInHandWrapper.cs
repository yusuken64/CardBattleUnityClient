using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class NumberOfCardsInHandWrapper : IValueProviderWrapperBase
{
    public CardBattleEngine.TeamRelationship TeamRelationship;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.NumberOfCardsInHand();
        instance.TeamRelationship = this.TeamRelationship;
        return instance;
    }
}