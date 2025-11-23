using CardBattleEngine;
using System;
using System.Linq;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    public string CardName;
    public string ID;
    public Sprite Sprite;
    public int Cost = 1;

	internal abstract CardBattleEngine.Card CreateCard();

    public string ActionToDescription(IGameActionWrapperBase action, int arg2)
	{
        IGameAction gameAction = action.Create();

        return gameAction switch
        {
            DamageAction dealDamage => $"Deal {dealDamage.Damage} damage",
            DrawCardFromDeckAction drawCard => $"Draw card",
            SummonMinionAction summon => $"Summon {summon.Card.Name}",
            FreezeAction freeze => $"Freeze",
            //HealAction heal => $"Heal {heal.Target} for {heal.Amount} HP",
            // Add more types as needed
            _ => action.Create().ToString() // fallback
        };
	}

    public string ToDescription(TriggeredEffectWrapper triggeredEffect, int arg2)
	{
        if (!string.IsNullOrWhiteSpace(triggeredEffect.Description))
		{
            return triggeredEffect.Description;
        }

        var trigger = triggeredEffect.EffectTrigger;
        var condition = "";
        if(triggeredEffect.Condition is not null)
        {
            condition = triggeredEffect.Condition.ToString() + ",";
        }

        string actions = string.Join(Environment.NewLine, triggeredEffect.GameActions.Select(ActionToDescription));
        string targeting = triggeredEffect.TargetType switch
		{
			TargetingType.Any => " to any target",
			TargetingType.FriendlyMinion => " to friendly minion",
			TargetingType.FriendlyHero => " to hero",
			TargetingType.EnemyMinion => " to minion",
			TargetingType.EnemyHero => " to opponent",
			TargetingType.AnyEnemy => " to target enemy",
			TargetingType.Self => " to self",
			TargetingType.None => "",
			TargetingType.AnyMinion => " to a minion",
			_ => throw new NotImplementedException(),
		};
        string description = $"{trigger}:{condition}{actions}{targeting}.";

        return description;
	}
}
