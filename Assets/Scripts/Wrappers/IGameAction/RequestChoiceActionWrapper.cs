using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class RequestChoiceActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.IPendingChoice PendingChoice;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RequestChoiceAction();
        instance.PendingChoice = this.PendingChoice;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}