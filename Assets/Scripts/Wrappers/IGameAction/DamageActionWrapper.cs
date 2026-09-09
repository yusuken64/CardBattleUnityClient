using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class DamageActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase Damage;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.DamageAction();
        instance.Damage = Damage?.Create();
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}