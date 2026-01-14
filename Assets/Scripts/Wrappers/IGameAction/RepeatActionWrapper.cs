using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class RepeatActionWrapper : IGameActionWrapperBase
{
    [SerializeReference] public IValueProviderWrapperBase Count;
    [SerializeReference]
    public List<IGameActionWrapperBase> ChildActions;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.RepeatAction();
        instance.Count = Count?.Create();
        instance.ChildActions = this.ChildActions?.Select(x => x.Create()).ToList();
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}