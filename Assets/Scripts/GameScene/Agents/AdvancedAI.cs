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
		var actionScores = GetTopActions(game);
		return (actionScores[0].Action, actionScores[0].Context);
	}

	public List<ActionScore> GetTopActions(GameState game, int topN = 10)
	{
		var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
		_cloneStopwatch = new System.Diagnostics.Stopwatch();
		_evalStopwatch = new System.Diagnostics.Stopwatch();

		Debug.Log($"AI thinking for {_player.Name}");
		_search = 0;

		var statePlayer = (CardBattleEngine.Player)game.GetEntityById(_player.Id);
		var actions = game.GetValidActions(statePlayer);

		var results = new List<ActionScore>();

		foreach (var action in actions)
		{
			_cloneStopwatch.Start();
			var simState = game.LightClone();
			_cloneStopwatch.Stop();

			var clonedContext = CloneContextFor(simState, action.Item2);
			var clonedAction = CloneActionFor(simState, action.Item1);

			var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id);

			if (!clonedAction.IsValid(simState, clonedContext, out _))
				continue;

			_engine.Resolve(simState, clonedContext, clonedAction);

			float score;

			if (action.Item1 is EndTurnAction)
				score = Evaluate(simState, simPlayer);
			else
				score = SearchScore(simState, 1);

			results.Add(new ActionScore
			{
				Action = action.Item1,
				Context = action.Item2,
				Score = score
			});
		}

		totalStopwatch.Stop();
		Debug.Log($"AI {totalStopwatch.ElapsedMilliseconds}ms, {_search} searches, eval {_evalStopwatch.ElapsedMilliseconds}ms, clone {_cloneStopwatch.ElapsedMilliseconds}ms");

		return results
			.OrderByDescending(r => r.Score)
			.Take(topN)
			.ToList();
	}

	private float SearchScore(GameState state, int depth)
	{
		_search++;

		var statePlayer = (CardBattleEngine.Player)state.GetEntityById(_player.Id);
		var actions = state.GetValidActions(statePlayer);

		if (depth > MaxDepth || actions.Count == 0)
			return Evaluate(state, statePlayer);

		float bestScore = float.NegativeInfinity;

		int k = GetBranchCount(actions.Count, depth);

		foreach (var action in actions.OrderBy(x => _random.Next()).Take(k))
		{
			_cloneStopwatch.Start();
			var simState = state.LightClone();
			_cloneStopwatch.Stop();

			var clonedContext = CloneContextFor(simState, action.Item2);
			var clonedAction = CloneActionFor(simState, action.Item1);
			var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id);

			if (!clonedAction.IsValid(simState, clonedContext, out _))
				continue;

			_engine.Resolve(simState, clonedContext, clonedAction);

			float score;

			if (action.Item1 is EndTurnAction)
				score = Evaluate(simState, simPlayer);
			else
				score = SearchScore(simState, depth + 1);

			bestScore = MathF.Max(bestScore, score);
		}

		return bestScore;
	}

	public static IGameAction CloneActionFor(GameState simState, IGameAction action)
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

	public static ActionContext CloneContextFor(GameState simState, ActionContext context)
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

	public float Evaluate(GameState state, CardBattleEngine.Player me)
	{
		var enemy = state.OpponentOf(me);

		float score = 0;

		// --------------------------------------------------
		// 0) TERMINAL STATES (must override everything)
		// --------------------------------------------------
		if (state.IsGameOver() &&
			state.Winner == me)
		{
			return 1_000_000f;   // we won
		}

		if (state.IsGameOver() &&
			state.Winner != me)
		{
			return -1_000_000f;  // we lost
		}

		// --------------------------------------------------
		// 1) Hero health + win pressure
		// --------------------------------------------------

		// Staying alive matters, but not as much as killing enemy
		score += me.Health * 2.5f;

		// Enemy HP is the win condition (increasing pressure as it drops)
		score -= enemy.Health * 6f;

		if (enemy.Health <= 15)
			score += (15 - enemy.Health) * 8f;

		if (enemy.Health <= 10)
			score += (10 - enemy.Health) * 15f;

		if (enemy.Health <= 5)
			score += (5 - enemy.Health) * 30f;


		// --------------------------------------------------
		// 2) Board presence (stats on board)
		// --------------------------------------------------

		float myBoardStats = me.Board.Sum(MinionValue);
		float enemyBoardStats = enemy.Board.Sum(MinionValue);

		score += myBoardStats;
		score -= enemyBoardStats;


		// --------------------------------------------------
		// 3) Future damage potential (NOT current attack availability)
		//    This fixes weapon-hoarding behaviour.
		// --------------------------------------------------

		float myNextTurnAttack =
			me.Board.Sum(m => m.Attack) +
			(me.EquippedWeapon != null ? me.EquippedWeapon.Attack : 0);

		float enemyNextTurnAttack =
			enemy.Board.Sum(m => m.Attack);

		score += myNextTurnAttack * 1.5f;
		score -= enemyNextTurnAttack * 2.0f;


		// --------------------------------------------------
		// 4) Encourage removing enemy damage sources
		//    This makes hero weapon swings valuable.
		// --------------------------------------------------

		float enemyBoardAttack = enemy.Board.Sum(m => m.Attack);
		score -= enemyBoardAttack * 1.2f;


		// --------------------------------------------------
		// 5) Taunts (board locking is valuable)
		// --------------------------------------------------

		float enemyTauntHealth = enemy.Board.Where(m => m.Taunt).Sum(m => m.Health);
		float myTauntHealth = me.Board.Where(m => m.Taunt).Sum(m => m.Health);

		score -= enemyTauntHealth * 1.5f;
		score += myTauntHealth * 1.5f;


		// --------------------------------------------------
		// 6) Card advantage
		// --------------------------------------------------

		score += me.Hand.Count * 1.5f;
		score -= enemy.Hand.Count * 1.5f;


		// --------------------------------------------------
		// 7) Immediate damage available THIS TURN
		//    Used only for lethal + face pressure
		// --------------------------------------------------

		float myReadyAttack =
			me.Board.Where(m => m.CanAttack()).Sum(m => m.Attack);

		if (me.CanAttack())
			myReadyAttack += me.Attack;

		float enemyReadyAttack =
			enemy.Board.Sum(m => m.Attack);

		// Reward pushing face when path is open
		if (enemyTauntHealth <= 0)
			score += myReadyAttack * 1.2f;


		// --------------------------------------------------
		// 8) Lethal checks (VERY IMPORTANT)
		// --------------------------------------------------

		if (myReadyAttack - enemyTauntHealth >= enemy.Health)
			score += 100_000;

		if (enemyReadyAttack - myTauntHealth >= me.Health)
			score -= 100_000;


		return score;
	}

	private float MinionValue(CardBattleEngine.Minion m)
	{
		float value = 0;

		// --------------------------------------------------
		// 1) Base stats (foundation of everything)
		// --------------------------------------------------
		value += m.Attack * 1.6f;
		value += m.Health * 1.2f;

		// Slight bonus for being healthy (harder to remove)
		value += MathF.Min(m.Health, m.MaxHealth) * 0.3f;


		// --------------------------------------------------
		// 2) Can it attack next turn?
		// --------------------------------------------------

		bool readyNextTurn =
			!m.HasSummoningSickness && !m.IsFrozen;

		if (readyNextTurn)
			value += m.Attack * 1.2f;   // guaranteed damage soon
		else
			value -= m.Attack * 0.6f;   // tempo loss


		// --------------------------------------------------
		// 3) Divine Shield = extra life bar
		// --------------------------------------------------
		if (m.HasDivineShield)
			value += m.Attack * 1.3f + m.Health * 1.3f;


		// --------------------------------------------------
		// 4) Taunt = protects hero + other minions
		// --------------------------------------------------
		if (m.Taunt)
			value += m.Health * 1.5f + 2.5f;


		// --------------------------------------------------
		// 5) Poisonous = kills ANY minion it touches
		// --------------------------------------------------
		if (m.HasPoisonous)
			value += 6f + m.Attack * 1.5f;


		// --------------------------------------------------
		// 6) Windfury = double damage potential
		// --------------------------------------------------
		if (m.HasWindfury)
			value += m.Attack * 1.4f;


		// --------------------------------------------------
		// 7) Stealth = guaranteed attack next turn
		// --------------------------------------------------
		if (m.IsStealth)
			value += m.Attack * 1.8f + 3f;


		// --------------------------------------------------
		// 8) Lifesteal = survivability + race potential
		// --------------------------------------------------
		if (m.HasLifeSteal)
			value += m.Attack * 1.0f;


		// --------------------------------------------------
		// 9) Reborn = second body after death
		// --------------------------------------------------
		if (m.HasReborn)
			value += m.Attack * 1.2f + m.Health * 1.2f + 4f;


		return value;
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
public class ActionScore
{
	public CardBattleEngine.IGameAction Action;
	public CardBattleEngine.ActionContext Context;
	public float Score;
}