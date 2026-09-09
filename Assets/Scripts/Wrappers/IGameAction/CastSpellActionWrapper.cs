using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class CastSpellActionWrapper : IGameActionWrapperBase
{
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.CastSpellAction();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}