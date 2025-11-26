using System;

[Serializable]
public abstract class IValueProviderWrapperBase
{
    public abstract CardBattleEngine.IValueProvider Create();
}