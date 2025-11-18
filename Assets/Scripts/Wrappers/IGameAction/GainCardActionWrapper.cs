using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class GainCardActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Card Card;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.GainCardAction();
        instance.Card = this.Card;
        instance.Canceled = this.Canceled;
        return instance;
    }
}