using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdvancedAI : IGameAgent
{
	private readonly CardBattleEngine.Player _player;
	private readonly IRNG _rng;
	private GameEngine _engine;
	private System.Random _random;

	int MaxDepth = 1;

	public AdvancedAI(CardBattleEngine.Player player, IRNG rng)
	{
		_player = player;
		_rng = rng;
		_engine = new CardBattleEngine.GameEngine();
		_engine.IsSimulation = true;
		_random = new System.Random();
	}

	private int _search;
	private System.Diagnostics.Stopwatch _cloneStopwatch;
	private System.Diagnostics.Stopwatch _evalStopwatch;

	public (IGameAction, ActionContext) GetNextAction(GameState game)
	{
		var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
		_cloneStopwatch = System.Diagnostics.Stopwatch.StartNew();
		_evalStopwatch = System.Diagnostics.Stopwatch.StartNew();
		Debug.Log($"AI thinking for {_player.Name}");
		_search = 0;

		var (_, bestAction) = SearchTurn(game, 0);

		totalStopwatch.Stop();
		Debug.Log($"AI {totalStopwatch.ElapsedMilliseconds}ms, {_search} search, eval {_evalStopwatch.ElapsedMilliseconds}ms, clone {_cloneStopwatch.ElapsedMilliseconds}ms");
		return bestAction;
	}

	private (float score, (IGameAction, ActionContext) bestAction) SearchTurn(GameState state, int depth)
	{
		_search++;
		var statePlayer = (CardBattleEngine.Player)state.GetEntityById(_player.Id);
		var actions = state.GetValidActions(statePlayer);

		float bestScore = float.NegativeInfinity;
		(IGameAction, ActionContext) bestAction = (null, null);

		int k = GetBranchCount(actions.Count(), depth);
		foreach (var action in actions.OrderBy(x => _random.Next()).Take(k))
		{
			_cloneStopwatch.Start();
			var simState = state.LightClone();
			_cloneStopwatch.Stop();

			ActionContext clonedContext = CloneContextFor(simState, action.Item2);
			IGameAction clonedAction = CloneActionFor(simState, action.Item1);
			var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id);

			var isValid = clonedAction.IsValid(simState, clonedContext, out string reason);
			if (!isValid)
			{
				continue;
			}

			_evalStopwatch.Start();
			_engine.Resolve(
				simState,
				clonedContext,
				clonedAction);
			_evalStopwatch.Stop();

			float score = 0;

			if (action.Item1 is EndTurnAction ||
				depth > MaxDepth)
			{
				score = Evaluate(simState, simPlayer);
			}
			else
			{
				var (childScore, _) = SearchTurn(simState, depth + 1);
				score = childScore;
			}

			if (score > bestScore)
			{
				bestScore = score;
				bestAction = action;
			}
		}

		return (bestScore, bestAction);
	}

	private IGameAction CloneActionFor(GameState simState, IGameAction action)
	{
		if (action is PlayCardAction playCardAction)
		{
			var newAction = new PlayCardAction();
			Guid id = playCardAction.Card.Id;
			newAction.Card = (CardBattleEngine.Card)simState.GetEntityById(id);
			return newAction;
		}

		return action;
	}

	private ActionContext CloneContextFor(GameState simState, ActionContext context)
	{
		return new ActionContext(context)
		{
			SourcePlayer = context.SourcePlayer == null ? null :
				(CardBattleEngine.Player)simState.GetEntityById(context.SourcePlayer.Id),

			SourceCard = context.SourceCard == null ? null :
				(CardBattleEngine.Card)simState.GetEntityById(context.SourceCard.Id),

			Source = context.Source == null ? null :
				simState.GetEntityById(context.Source.Id),

			Target = context.Target == null ? null :
				simState.GetEntityById(context.Target.Id),
		};
	}

	private float Evaluate(GameState state, CardBattleEngine.Player me)
	{
		var enemy = state.OpponentOf(me);

		float score = 0;

		// 1) Hero health (winning/losing matters most)
		//score += me.Health * 10;
		score -= enemy.Health * 10;

		// 2) Board presence (stats on board)
		score += me.Board.Sum(m => m.Attack * 1.5f + m.Health * 1.0f);
		score -= enemy.Board.Sum(m => m.Attack * 1.5f + m.Health * 1.0f);

		// 3) Immediate damage available THIS TURN (tempo)
		var myReadyAttack = me.Board.Where(m => m.CanAttack()).Sum(m => m.Attack);
		if (me.CanAttack()) { myReadyAttack += me.Attack; }
		var enemyReadyAttack = enemy.Board.Sum(m => m.Attack); //all enemy minion can attack next turn

		score += myReadyAttack * 2.0f;
		score -= enemyReadyAttack * 2.0f;

		// 4) Taunts (board locking is very valuable)
		var enemyTauntHealth = enemy.Board.Where(m => m.Taunt).Sum(m => m.Health);
		var myTauntHealth = me.Board.Where(m => m.Taunt).Sum(m => m.Health);

		score -= enemyTauntHealth * 1.5f;
		score += myTauntHealth * 1.5f;

		// 5) Card advantage
		score += me.Hand.Count * 1.5f;
		score -= enemy.Hand.Count * 1.5f;

		// 6) Lethal bonus (VERY IMPORTANT)
		if (myReadyAttack - enemyTauntHealth >= enemy.Health)
			score += 100_000;

		if (enemyReadyAttack - myTauntHealth >= me.Health)
			score -= 100_000;

		return score;
	}

	public T ChooseRandom<T>(IReadOnlyList<T> options)
	{
		if (options.Count == 0) return default!;
		return options[_rng.NextInt(0, options.Count)];
	}

	private int GetBranchCount(int totalActions, int depth)
	{
		if (depth == 0) return totalActions; // full width root

		// exponential decay
		double factor = Math.Pow(0.25, depth);
		int count = (int)Math.Ceiling(totalActions * factor);

		return Math.Max(1, count);
	}

	public void OnGameEnd(GameState gamestate, bool win)
	{
	}
}