using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class SpendManaActionWrapper : IGameActionWrapperBase
{
    public System.Int32 Amount;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.SpendManaAction();
        instance.Amount = this.Amount;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}