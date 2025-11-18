using System;

[Serializable]
public abstract class IAffectedEntitySelectorWrapperBase
{
    public abstract CardBattleEngine.IAffectedEntitySelector Create();
}
