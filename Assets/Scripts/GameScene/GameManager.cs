using CardBattleEngine;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player Player;
    public Player Opponent;

	public PlayResolver PlayResolver;
	public AnimationQueue AnimationQueue;

	public GameEngine _engine { get; private set; }

	private RandomAI _opponentAgent;

	public GameState _gameState { get; private set; }


	void Start()
    {
		ClearBoard();
        InitializeGame();
    }

	private void ClearBoard()
	{
		Player.Clear();
		Opponent.Clear();
	}

	private void InitializeGame()
	{
		_engine = new GameEngine(new UnityRNG());

		_gameState = GameFactory.CreateTestGame();
		Player.Data = _gameState.Players[0];
		Opponent.Data = _gameState.Players[0];
		_opponentAgent = new RandomAI(Opponent.Data, new UnityRNG());

		_engine.ActionPlaybackCallback = ActionPlaybackCallback;
		_engine.ActionResolvedCallback = ActionResolvedCallback;
		_engine.StartGame(_gameState);
	}

	public bool ChecksValid(IGameAction action, ActionContext context)
	{
		return action.IsValid(_gameState, context);
	}

	public void ResolveAction(IGameAction action, ActionContext context)
	{
		if (action.IsValid(_gameState, context))
		{
			_engine.Resolve(_gameState, context, action);
		}
	}

	internal Player GetPlayerFor(CardBattleEngine.Player sourcePlayer)
	{
		return Player.Data.Name == sourcePlayer.Name ? Player : Opponent;
	}

	internal CardBattleEngine.Player GetDataFor(GameState state, Player player)
	{
		return Player.Data.Name == state.Players[0].Name ? state.Players[0] : state.Players[1];
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		Debug.Log(current);
		AnimationQueue.EnqueueAnimation(this, state, current);
	}

	private void ActionResolvedCallback(GameState state)
	{
		if (state.CurrentPlayer.Name == Opponent.Data.Name)
		{
			(IGameAction, ActionContext) nextAction = ((IGameAgent)_opponentAgent).GetNextAction(state);
			ResolveAction(nextAction.Item1, nextAction.Item2);
		}
	}
}

internal class UnityRNG : IRNG
{
	public UnityRNG()
	{
	}

	public IRNG Clone()
	{
		return new UnityRNG();//This doesn't work
	}

	public double NextDouble()
	{
		return UnityEngine.Random.Range(0.0f, float.MaxValue);
	}

	public int NextInt(int maxExclusive)
	{
		return UnityEngine.Random.Range(0, maxExclusive);
	}

	public int NextInt(int minInclusive, int maxExclusive)
	{
		return UnityEngine.Random.Range(minInclusive, maxExclusive);
	}
}