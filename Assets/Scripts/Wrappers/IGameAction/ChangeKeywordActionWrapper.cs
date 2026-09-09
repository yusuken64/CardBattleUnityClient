using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ChangeKeywordActionWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.ChangeType ChangeType;
    public CardBattleEngine.Keyword Keyword;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.ChangeKeywordAction();
        instance.ChangeType = this.ChangeType;
        instance.Keyword = this.Keyword;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}