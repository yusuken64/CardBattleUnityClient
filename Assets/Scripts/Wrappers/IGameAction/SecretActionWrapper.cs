using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SecretActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Secret Secret;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SecretAction();
        instance.Secret = this.Secret;
        instance.Canceled = this.Canceled;
        return instance;
    }
}