using System;

[Serializable]
public abstract class ITargetOperationWrapperBase
{
    public abstract CardBattleEngine.ITargetOperation Create();
}