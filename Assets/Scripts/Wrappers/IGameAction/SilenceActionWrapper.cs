using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SilenceActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SilenceAction();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}