using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class HealActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase Amount;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.HealAction();
        instance.Amount = Amount?.Create();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}