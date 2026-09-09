using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SecretResolvedActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Secret Secret;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SecretResolvedAction();
        instance.Secret = this.Secret;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}