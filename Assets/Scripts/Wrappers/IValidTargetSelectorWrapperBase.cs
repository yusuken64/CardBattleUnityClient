using System;

[Serializable]
public abstract class IValidTargetSelectorWrapperBase
{
    public abstract CardBattleEngine.IValidTargetSelector Create();
}