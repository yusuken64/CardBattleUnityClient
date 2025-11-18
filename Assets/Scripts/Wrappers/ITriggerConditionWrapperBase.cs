using System;

[Serializable]
public abstract class ITriggerConditionWrapperBase
{
    public abstract CardBattleEngine.ITriggerCondition Create();
}
