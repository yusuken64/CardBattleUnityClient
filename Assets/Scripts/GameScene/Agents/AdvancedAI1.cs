using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class AdvancedAI1 : IGameAgent
{
	private CardBattleEngine.Player _player;
	private readonly IRNG _rng;
	private GameEngine _engine;
	private System.Random _random;

	const int BEAM_WIDTH = 8;
	const int MAX_DEPTH = 4;

	public AdvancedAI1(CardBattleEngine.Player player, IRNG rng)
	{
		_player = player;
		_rng = rng;
		_engine = new CardBattleEngine.GameEngine();
		_engine.IsSimulation = true;
		_random = new System.Random();
	}

	private int _search;
	private int _cacheHit;
	private System.Diagnostics.Stopwatch _totalStopWatch = new();
	private Dictionary<ulong, float> _gameState_Hash_Score_Cache = new();

	public (IGameAction, ActionContext) GetNextAction(GameState game)
	{
		_search = 0;
		_cacheHit = 0;
		_gameState_Hash_Score_Cache.Clear();
		_totalStopWatch.Start();
		var actionScores = GetTopActions(game);
		_totalStopWatch.Stop();

		Debug.Log($"search {_search}, cached {_cacheHit}, {_totalStopWatch.ElapsedMilliseconds}ms");
		return (actionScores[0].RootAction, actionScores[0].RootContext);
	}

	public List<BeamNode> GetTopActions(GameState game)
	{
		var statePlayer = (CardBattleEngine.Player)game.GetEntityById(_player.Id);
		var rootActions = game.GetValidActions(statePlayer);

		var beam = new List<BeamNode>();

		foreach (var action in rootActions)
		{
			var simState = game.LightClone();
			var clonedContext = CloneContextFor(simState, action.Item2);
			var clonedAction = CloneActionFor(simState, action.Item1);
			var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id);

			if (!clonedAction.IsValid(simState, clonedContext, out _))
				continue;

			_engine.Resolve(simState, clonedContext, clonedAction);

			beam.Add(new BeamNode
			{
				SimState = simState,
				SimPlayer = simPlayer,
				Depth = 1,
				RootAction = action.Item1,
				RootContext = action.Item2,
				Score = Evaluate(simState, simPlayer)
			});
		}

		for (int depth = 2; depth <= MAX_DEPTH; depth++)
		{
			var candidates = new List<BeamNode>();

			foreach (var node in beam)
			{
				var actions = node.SimState.GetValidActions(node.SimPlayer);

				foreach (var action in actions)
				{
					var simState = node.SimState.LightClone();
					var clonedContext = CloneContextFor(simState, action.Item2);
					var clonedAction = CloneActionFor(simState, action.Item1);
					var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(node.SimPlayer.Id);

					if (!clonedAction.IsValid(simState, clonedContext, out _))
						continue;

					_engine.Resolve(simState, clonedContext, clonedAction);

					float score = SearchScore(simState, depth + 1);
					//float score = Evaluate(simState, simPlayer);

					candidates.Add(new BeamNode
					{
						SimState = simState,
						SimPlayer = simState.CurrentPlayer,
						Depth = depth,
						RootAction = node.RootAction,
						RootContext = node.RootContext,
						Score = score
					});
				}
			}

			if (candidates.Count == 0)
				break;

			// THE BEAM PRUNE (this is the core of beam search)
			beam = candidates
				.OrderByDescending(n => n.Score)
				.Take(BEAM_WIDTH)
				.ToList();
		}

		return beam;
	}

	bool AllSameRoot(List<BeamNode> beam)
	{
		var first = beam[0].RootAction;
		return beam.All(n => n.RootAction == first);
	}

	private float SearchScore(GameState state, int depth)
	{
		var statePlayer = (CardBattleEngine.Player)state.GetEntityById(_player.Id);
		var hash = GameStateHasher.HashState(state, statePlayer);
		if (_gameState_Hash_Score_Cache.TryGetValue(hash, out float value))
		{
			_cacheHit++;
			return value;
		}
		_search++;

		var actions = state.GetValidActions(statePlayer);

		if (depth > MAX_DEPTH || actions.Count == 0)
			return Evaluate(state, statePlayer);

		float bestScore = float.NegativeInfinity;

		int k = GetBranchCount(actions.Count, depth);

		foreach (var action in actions.OrderBy(x => _random.Next()).Take(k))
		{
			var simState = state.LightClone();

			var clonedContext = CloneContextFor(simState, action.Item2);
			var clonedAction = CloneActionFor(simState, action.Item1);
			var simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id);

			if (!clonedAction.IsValid(simState, clonedContext, out _))
				continue;

			_engine.Resolve(simState, clonedContext, clonedAction);

			float score;

			if (action.Item1 is EndTurnAction)
			{
				score = Evaluate(simState, simPlayer);
			}
			else
			{
				score = SearchScore(simState, depth + 1);
			}

			var simHash = GameStateHasher.HashState(simState, simPlayer);
			_gameState_Hash_Score_Cache[simHash] = score;
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

		// Reward having extra mana next turn
		float myFutureMana = me.MaxMana - enemy.MaxMana;
		score += myFutureMana * 5f;

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
		// configurable knobs
		const int rootWidth = 12;      // how many moves to consider at root
		const int minWidth = 2;        // never go below this (until late)
		const double decay = 0.6;      // how fast width shrinks

		if (depth == 0)
			return totalActions; // still search all root moves for scoring

		int width = (int)Math.Ceiling(rootWidth * Math.Pow(decay, depth - 1));

		width = Math.Max(minWidth, width);

		// can't exceed available actions
		return Math.Min(totalActions, width);
	}

	public void OnGameEnd(GameState gamestate, bool win)
	{
	}
	public void SetPlayer(CardBattleEngine.Player player)
	{
		_player = player;
	}
}

public class BeamNode
{
	public GameState SimState;
	public CardBattleEngine.Player SimPlayer;
	public float Score;
	public int Depth;

	// Needed so we can return the best ROOT action at the end
	public IGameAction RootAction;
	public ActionContext RootContext;
}
