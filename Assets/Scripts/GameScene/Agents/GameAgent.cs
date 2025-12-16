using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameAgent : MonoBehaviour
{
}

public interface IGameAgent
{
    public (IGameAction, ActionContext) GetNextAction(GameState game);

    public void OnGameEnd(GameState gamestate, bool win);
	void SetTarget((IGameAction, ActionContext) nextAction, Func<TargetingType, ITriggerSource> targetSelector);
}

public class RandomAI : IGameAgent
{
	private readonly CardBattleEngine.Player _player;
	private readonly IRNG _rng;

	public RandomAI(CardBattleEngine.Player player, IRNG rng)
	{
		_player = player;
		_rng = rng;
	}

	(IGameAction, ActionContext) IGameAgent.GetNextAction(GameState game)
	{
		var validActions = game.GetValidActions(_player)
			.Where(x => x.Item1.IsValid(game, x.Item2))
			.ToList();

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
		if (action is PlayCardAction playCardAction)
		{
			card = playCardAction.Card;
		}
		else
		{
			return;
		}

		TargetingType targetingType = TargetingType.None;
		if (card is MinionCard minionCard)
		{
			targetingType = minionCard.MinionTriggeredEffects[0].TargetType;
		}
		else if (card is SpellCard spellCard)
		{
			targetingType = spellCard.TargetingType;
		}
		else if (card is WeaponCard weaponCard)
		{
			targetingType = TargetingType.FriendlyHero;
		}

		var target = targetSelector?.Invoke(targetingType);
		context.Target = target?.Entity;
	}
}
