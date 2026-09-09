using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class IncreaseMaxManaActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Amount;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.IncreaseMaxManaAction();
        instance.Amount = this.Amount;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}