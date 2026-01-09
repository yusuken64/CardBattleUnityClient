using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class BasicAI : IGameAgent
{
	private readonly CardBattleEngine.Player _player;
	private readonly IRNG _rng;

	public BasicAI(CardBattleEngine.Player player, IRNG rng)
	{
		_player = player;
		_rng = rng;
	}

	public (IGameAction, ActionContext) GetNextAction(GameState game)
	{
		var validActions = game.GetValidActions(_player);

		var attackActions = validActions.Where(x => x.Item1 is AttackAction attackAction).ToList();
		if (attackActions.Any())
		{
			return ChooseRandom(attackActions);
		}

		return ChooseRandom(validActions);
	}

	public T ChooseRandom<T>(IReadOnlyList<T> options)
	{
		if (options.Count == 0) return default!;
		return options[_rng.NextInt(0, options.Count)];
	}

	public void OnGameEnd(GameState gamestate, bool win)
	{
	}

	public void SetTarget((IGameAction, ActionContext) nextAction, Func<TargetingType, ITriggerSource> targetSelector)
	{
		var action = nextAction.Item1;
		var context = nextAction.Item2;

		CardBattleEngine.Card card;
		if (!(action is PlayCardAction playCardAction))
		{
			return;
		}

		card = playCardAction.Card;

		TargetingType targetingType = TargetingType.None;
		if (card is MinionCard minionCard)
		{
			if (minionCard.MinionTriggeredEffects.Any())
			{
				targetingType = minionCard.MinionTriggeredEffects[0].TargetType;
			}
		}
		else if (card is SpellCard spellCard)
		{
			targetingType = spellCard.TargetingType;
		}
		else if (card is WeaponCard weaponCard)
		{
			targetingType = TargetingType.FriendlyHero;
		}

		context.Source = card;
		var target = targetSelector?.Invoke(targetingType);
		context.Target = target?.Entity;
	}
}
