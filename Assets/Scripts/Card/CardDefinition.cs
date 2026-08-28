using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
			HealAction heal => DescribeHeal(heal),
			DrawCardFromDeckAction drawCard => $"Draw card",
			SummonMinionAction summon => $"Summon {summon.Card.Name}",
			FreezeAction freeze => $"Freeze",
			AddStatModifierAction addStat => DescribeStatMod(addStat),
			GainCardAction gainCard => $"Gain {gainCard.Card.Name}",
			GainArmorAction gainArmor => $"Gain {GetValue(gainArmor.Amount)} Armor",
			RepeatAction repeatAction => $"{string.Join(", ", repeatAction.ChildActions.Select(x => ActionToDescription(x)))} x {((ConstantValue)repeatAction.Count).Number}",
			SilenceAction silenceAction => $"Silence",
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

	string DescribeHeal(HealAction h)
	{
		var healString = h.Amount switch
		{
			ConstantValue value => $"Heal {value.Number}",
			_ => h.GetType().Name,
		};
		return healString;
	}

	string GetValue(IValueProvider valueProvider)
	{
		string value = valueProvider switch
		{
			ConstantValue constant => constant.Number.ToString(),
			_ => valueProvider.GetType().Name,
		};

		return value;
	}

	public virtual string ToDescription(TriggeredEffectWrapper triggeredEffect, int arg2)
	{
		if (!string.IsNullOrWhiteSpace(triggeredEffect.Description))
		{
			return triggeredEffect.Description;
		}

		var trigger = AddSpacesToCamelCase(triggeredEffect.EffectTrigger.ToString());
		var condition = "";
		//if (triggeredEffect.Condition is not null)
		//{
		//	string text = triggeredEffect.Condition.GetType().Name;
		//	string suffix = "ConditionWrapper";
		//	if (text.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
		//	{
		//		text = text.Substring(0, text.Length - suffix.Length);
		//	}

		//	condition = text + ",";
		//}

		string actions = string.Join(Environment.NewLine, triggeredEffect.GameActions.Select(ActionWrapperToDescription));
		string description = $"{trigger}: {condition}{actions}.";

		return description;
	}

	public static string AddSpacesToCamelCase(string input)
	{
		if (string.IsNullOrEmpty(input))
			return input;

		var sb = new StringBuilder(input.Length + 5);

		sb.Append(input[0]);

		for (int i = 1; i < input.Length; i++)
		{
			char current = input[i];
			char previous = input[i - 1];

			// Add space if:
			// - current is uppercase AND
			// - previous is lowercase OR next is lowercase (handles "XMLParser" -> "XML Parser")
			if (char.IsUpper(current) &&
				(char.IsLower(previous) ||
				 (i + 1 < input.Length && char.IsLower(input[i + 1]))))
			{
				sb.Append(' ');
			}

			sb.Append(current);
		}

		return sb.ToString();
	}
}
