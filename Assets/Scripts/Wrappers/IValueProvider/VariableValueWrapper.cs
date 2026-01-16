using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class VariableValueWrapper : IValueProviderWrapperBase
{
    public System.String VariableName;

    public override CardBattleEngine.IValueProvider Create()
    {
        var instance = new CardBattleEngine.VariableValue();
        instance.VariableName = this.VariableName;
        return instance;
    }
}