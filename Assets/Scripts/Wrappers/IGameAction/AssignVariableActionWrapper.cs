using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class AssignVariableActionWrapper : IGameActionWrapperBase
{
    public System.String VariableName;
    [SerializeReference] public IValueProviderWrapperBase Value;
    public CardBattleEngine.VariableScope VariableScope;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.AssignVariableAction();
        instance.VariableName = this.VariableName;
        instance.Value = Value?.Create();
        instance.VariableScope = this.VariableScope;
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}