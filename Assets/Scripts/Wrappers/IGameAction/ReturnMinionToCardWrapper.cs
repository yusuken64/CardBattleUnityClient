using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;

[Serializable]
public class ReturnMinionToCardWrapper : IGameActionWrapperBase
{
    public CardBattleEngine.TeamRelationship TeamRelationship;
    public CardBattleEngine.ZoneType ZoneType;
    public System.Boolean Canceled;
    public CustomSFX CustomSFX;

    public override CardBattleEngine.IGameAction Create()
    {
        var instance = new CardBattleEngine.ReturnMinionToCard();
        instance.TeamRelationship = this.TeamRelationship;
        instance.ZoneType = this.ZoneType;
        instance.Canceled = this.Canceled;
        instance.CustomSFX = CustomSFX;
        return instance;
    }
}