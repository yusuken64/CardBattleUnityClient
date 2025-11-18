using CardBattleEngine;
using System.Collections.Generic;
using UnityEngine;

public class GameAgent : MonoBehaviour
{
}

public interface IGameAgent
{
    public (IGameAction, ActionContext) GetNextAction(GameState game);

    public void OnGameEnd(GameState gamestate, bool win);
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
		var validActions = game.GetValidActions(_player);

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
