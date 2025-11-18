using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class PlayCardActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.PlayCardAction();
        instance.Canceled = this.Canceled;
        return instance;
    }
}