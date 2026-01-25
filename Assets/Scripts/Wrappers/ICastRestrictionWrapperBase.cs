using System;

[Serializable]
public abstract class ICastRestrictionWrapperBase
{
    public abstract CardBattleEngine.ICastRestriction Create();
}
