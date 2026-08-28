using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SequentialActionWrapper : IGameActionWrapperBase
{
    public List<SequentialEffectWrapper> Effects;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SequentialAction();
        instance.Effects = Effects?.Select(x => x.Create()).ToList();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}