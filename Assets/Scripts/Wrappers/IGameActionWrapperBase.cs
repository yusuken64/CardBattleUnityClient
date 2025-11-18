using System;

[Serializable]
public abstract class IGameActionWrapperBase
{
    public abstract CardBattleEngine.IGameAction Create();
}
