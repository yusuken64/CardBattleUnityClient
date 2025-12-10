using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SubmitMulliganActionWrapper : IGameActionWrapperBase
{
    public System.Collections.Generic.List<CardBattleEngine.Card> CardsToReplace;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SubmitMulliganAction();
        instance.CardsToReplace = this.CardsToReplace;
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}