using System;
using UnityEngine;
using CardBattleEngine;

[Serializable]
public class StartGameActionWrapper : IGameActionWrapperBase
{
    public System.Boolean Canceled;

    public override CardBattleEngine.IGameAction Create()
    {
        return null;
        //var instance = new CardBattleEngine.StartGameAction();
        //instance.Canceled = this.Canceled;
        //return instance;
    }
}