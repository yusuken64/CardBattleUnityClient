using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class GainArmorActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase Amount;
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.GainArmorAction();
        instance.Amount = Amount?.Create();
        instance.Canceled = this.Canceled;
        return instance;
    }
}