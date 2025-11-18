using CardBattleEngine;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player Player;
    public Player Opponent;

	public PlayResolver PlayResolver;
	public AnimationQueue AnimationQueue;

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
		GameEngine _engine = new(new UnityRNG());

		CardBattleEngine.Player p1 = new("Alice");
		CardBattleEngine.Player p2 = new("Bob");
		Player.Data = p1;
		Opponent.Data = p2;
		//GameState _state = new GameState(p1, p2);
		var gameState = GameFactory.CreateTestGame();

		_engine.ActionPlaybackCallback = ActionPlaybackCallback;
		_engine.StartGame(gameState);
	}

	internal Player GetPlayerFor(CardBattleEngine.Player sourcePlayer)
	{
		return Player.Data.Name == sourcePlayer.Name ? Player : Opponent;
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		Debug.Log(current);
		AnimationQueue.EnqueueAnimation(this, state, current);
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