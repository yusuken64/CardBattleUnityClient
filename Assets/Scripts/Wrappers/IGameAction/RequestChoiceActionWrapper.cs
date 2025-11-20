using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class RequestChoiceActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.IPendingChoice PendingChoice;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RequestChoiceAction();
        instance.PendingChoice = this.PendingChoice;
        instance.Canceled = this.Canceled;
        return instance;
    }
}