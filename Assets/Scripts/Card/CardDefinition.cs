using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
	public bool Collectable;
	public string CardName;
	public string ID;
	public Sprite Sprite;
	public int Cost = 1;

	[SerializeReference]
	public ICastRestrictionWrapperBase CastRestriction;
	[SerializeReference]
	public IValidTargetSelectorWrapperBase ValidTargetSelector;

	[SerializeReference]
	public List<TriggeredEffectWrapper> TriggeredEffects = new List<TriggeredEffectWrapper>();

	public abstract CardBattleEngine.Card CreateCard();

	public string ActionWrapperToDescription(IGameActionWrapperBase action, int arg2)
	{
		IGameAction gameAction = action.Create();

		return ActionToDescription(gameAction);
	}

	private string ActionToDescription(IGameAction gameAction)
	{
		return gameAction switch
		{
			DamageAction dealDamage => DescribeDamage(dealDamage),
			DrawCardFromDeckAction drawCard => $"Draw card",
			SummonMinionAction summon => $"Summon {summon.Card.Name}",
			FreezeAction freeze => $"Freeze",
			AddStatModifierAction addStat => DescribeStatMod(addStat),
			GainCardAction gainCard => $"Gain {gainCard.Card.Name}",
			RepeatAction repeatAction => $"{string.Join(", ", repeatAction.ChildActions.Select(x => ActionToDescription(x)))} x {((ConstantValue)repeatAction.Count).Number}",
			//HealAction heal => $"Heal {heal.Target} for {heal.Amount} HP",
			// Add more types as needed
			_ => gameAction.GetType().Name.ToString() // fallback
		};
	}

	private string DescribeStatMod(AddStatModifierAction addStat)
	{
		var attack = addStat.AttackChange is ConstantValue a ? a.Number : 0;
		var health = addStat.HealthChange is ConstantValue h ? h.Number : 0;

		return $"{FormatStat(attack)}/{FormatStat(health)}";
	}

	private string FormatStat(int value)
	{
		return value >= 0 ? $"+{value}" : value.ToString();
	}

	string DescribeDamage(DamageAction d)
	{
		var damageString = d.Damage switch
		{
			ConstantValue value => $"Deal {value.Number} damage",
			_ => d.GetType().Name,
		};
		return damageString;
	}

	public virtual string ToDescription(TriggeredEffectWrapper triggeredEffect, int arg2)
	{
		if (!string.IsNullOrWhiteSpace(triggeredEffect.Description))
		{
			return triggeredEffect.Description;
		}

		var trigger = triggeredEffect.EffectTrigger;
		var condition = "";
		if (triggeredEffect.Condition is not null)
		{
			string text = triggeredEffect.Condition.GetType().Name;
			string suffix = "ConditionWrapper";
			if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			{
				text = text.Substring(0, text.Length - suffix.Length);
			}

			condition = text + ",";
		}

		string actions = string.Join(Environment.NewLine, triggeredEffect.GameActions.Select(ActionWrapperToDescription));
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
		string description = $"{trigger}: {condition}{actions}{targeting}.";

		return description;
	}
}
