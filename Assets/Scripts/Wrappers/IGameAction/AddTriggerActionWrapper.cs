using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AddTriggerActionWrapper : IGameActionWrapperBase
{
    public TriggeredEffectWrapper Effect;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AddTriggerAction();
        instance.Effect = Effect?.CreateEffect();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}