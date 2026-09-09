using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class PlayCardActionWrapper : IGameActionWrapperBase
{
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.PlayCardAction();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}