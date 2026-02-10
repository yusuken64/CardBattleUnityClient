using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
			.Where(x => x.Item1.IsValid(game, x.Item2, out string _))
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
}
