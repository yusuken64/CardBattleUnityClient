using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class SecretResolvedActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.Secret Secret;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SecretResolvedAction();
        instance.Secret = this.Secret;
        instance.Canceled = this.Canceled;
        return instance;
    }
}