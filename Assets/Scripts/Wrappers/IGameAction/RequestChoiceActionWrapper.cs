using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class RequestChoiceActionWrapper : IGameActionWrapperBase
{
    public IPendingChoiceWrapperBase PendingChoice;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RequestChoiceAction();
        instance.PendingChoice = this.PendingChoice?.Create();
        instance.Canceled = this.Canceled;
        return instance;
    }
}