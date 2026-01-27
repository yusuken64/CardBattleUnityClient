using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class KeywordSelectorWrapper : IValidTargetSelectorWrapperBase
{
    public CardBattleEngine.Keyword Keyword;
    public System.Boolean HasKeyword;

    public override CardBattleEngine.IValidTargetSelector Create()
    {
        var instance = new CardBattleEngine.KeywordSelector();
        instance.Keyword = this.Keyword;
        instance.HasKeyword = this.HasKeyword;
        return instance;
    }
}