using CardBattleEngine;
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

		var actions = validActions
			.Where(x => x.Item1 is AttackAction attackAction ||
						x.Item1 is PlayCardAction).ToList();
		if (actions.Any())
		{
			return ChooseRandom(actions);
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
}
