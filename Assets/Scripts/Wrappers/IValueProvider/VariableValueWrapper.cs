using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class VariableValueWrapper : IValueProviderWrapperBase
{
    public System.String VariableName;
    public CardBattleEngine.VariableScope VariableScope;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.VariableValue();
        instance.VariableName = this.VariableName;
        instance.VariableScope = this.VariableScope;
        return instance;
    }
}