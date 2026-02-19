using CardBattleEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MonteCarloAgent : IGameAgent
{
	private CardBattleEngine.Player _player;
	private readonly IRNG _rng;
	private GameEngine _engine;
	private System.Random _random;

	private GameEngine _gameEngine;
	private int MAXSTEPS = 2000;

	public MonteCarloAgent(CardBattleEngine.Player player, IRNG rng, int maxSteps)
	{
		_player = player;
		_rng = rng;
		_engine = new CardBattleEngine.GameEngine();
		_engine.IsSimulation = true;
		_random = new System.Random();
		_gameEngine = new();
		MAXSTEPS = maxSteps;
	}

	public void SetPlayer(CardBattleEngine.Player player)
	{
		_player = player;
	}

	private int _search;
	private int _cacheHit;
	private System.Diagnostics.Stopwatch _totalStopWatch = new();
	private Dictionary<ulong, int> _gameState_Hash_Score_Cache = new();

	public (IGameAction, ActionContext) GetNextAction(GameState game)
	{
		_search = 0;
		_cacheHit = 0;
		_gameState_Hash_Score_Cache.Clear();
		_totalStopWatch.Start();
		var topAction = GetTopAction(game);
		_totalStopWatch.Stop();

		var nextContext = CloneContextFor(game, topAction.clonedContext);
		var nextAction = CloneActionFor(game, topAction.clonedAction);
		//Debug.Log($"search {_search}, cached {_cacheHit}, {_totalStopWatch.ElapsedMilliseconds}ms");
		return (nextAction, nextContext);
	}

	private MonteCarloNode GetTopAction(GameState game)
	{
		var statePlayer = (CardBattleEngine.Player)game.GetEntityById(_player.Id);

		GameState simState = game.LightClone();
		MonteCarloNode root = new MonteCarloNode()
		{
			engine = _gameEngine,
			gameState_Hash_Score_Cache = _gameState_Hash_Score_Cache,
			simState = simState,
			//clonedContext = CloneContextFor(simState, rootAction.Item2),
			//clonedAction = CloneActionFor(simState, rootAction.Item1),
			simPlayer = (CardBattleEngine.Player)simState.GetEntityById(statePlayer.Id),
			random = _random,
			Visits = 0,
			Wins = 0,
		};

		for (int i = 0; i < MAXSTEPS; i++)
		{
			MonteCarloNode node = root;

			// 1) SELECTION
			while (node.IsFullyExpanded &&
				node.Children.Count > 0 &&
				!node.simState.IsGameOver())
				node = node.SelectBestChild();

			// 2) EXPANSION
			if (!node.simState.IsGameOver())
				node = node.ExpandOneChild();

			// 3) SIMULATION
			_search++;
			node.RollOut();
		}

		return root.Children
			.OrderByDescending(c => c.Visits)
			.First();
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

	public void OnGameEnd(GameState gamestate, bool win)
	{
	}

	public class MonteCarloNode
	{
		public MonteCarloNode Parent;
		public List<MonteCarloNode> Children = new();
		private List<(IGameAction, ActionContext)> _untriedActions;

		public int Visits;
		public int Wins;

		internal GameEngine engine;
		internal GameState simState;
		internal ActionContext clonedContext;
		internal IGameAction clonedAction;
		internal CardBattleEngine.Player simPlayer;
		internal System.Random random;
		internal Dictionary<ulong, int> gameState_Hash_Score_Cache;
		private readonly float C = 2;

		public bool IsFullyExpanded { get; internal set; }

		public void RollOut()
		{
			var hash = GameStateHasher.HashState(simState, simState.CurrentPlayer);

			int score;
			while (!simState.IsGameOver())
			{
				var validActions = simState.GetValidActions(simState.CurrentPlayer);
				(IGameAction, ActionContext) bestAction;
				if (simState.CurrentPlayer.Id == simPlayer.Id)
				{
					// pick best action according to heuristic
					float bestScore = float.NegativeInfinity;
					bestAction = validActions[0];

					foreach (var actionPair in validActions)
					{
						var clone = simState.LightClone();
						var ctx = MonteCarloAgent.CloneContextFor(clone, actionPair.Item2);
						var act = MonteCarloAgent.CloneActionFor(clone, actionPair.Item1);
						var clonePlayer = simState.GetEntityById(simPlayer.Id) as CardBattleEngine.Player;

						engine.Resolve(clone, ctx, act);

						float evalscore = Evaluate(clone, clonePlayer);

						if (evalscore > bestScore)
						{
							bestScore = evalscore;
							bestAction = actionPair;
						}
					}
				}
				else
				{
					bestAction = validActions[random.Next(0, validActions.Count)];
				}

				var bestCtx = MonteCarloAgent.CloneContextFor(simState, bestAction.Item2);
				var bestAct = MonteCarloAgent.CloneActionFor(simState, bestAction.Item1);
				engine.Resolve(simState, bestCtx, bestAct);
			}

			score = simState.Winner == simPlayer ? 1 : -1;

			gameState_Hash_Score_Cache[hash] = score;

			BackProp(score);
		}

		public float Evaluate(GameState state, CardBattleEngine.Player me)
		{
			return AdvancedAI.Evaluate(state, me);
		}

		private void BackProp(int score)
		{
			this.Wins += score;
			this.Visits++;

			this.Parent?.BackProp(score);
		}

		public float CalcualteUCB()
		{
			if (Visits == 0)
				return float.PositiveInfinity;

			if (Parent == null)
				return Wins / (float)Visits;

			float exploitation = Wins / (float)Visits;
			float exploration = C * Mathf.Sqrt(Mathf.Log(Parent.Visits) / Visits);
			return exploitation + exploration;
		}

		private void EnsureActionList()
		{
			if (_untriedActions != null)
				return;

			_untriedActions = simState.GetValidActions(simState.CurrentPlayer).ToList();

			if (_untriedActions.Count == 0)
				IsFullyExpanded = true;
		}

		internal MonteCarloNode SelectBestChild()
		{
			return Children
				.OrderByDescending(c => c.CalcualteUCB())
				.First();
		}

		internal MonteCarloNode ExpandOneChild()
		{
			EnsureActionList();

			if (_untriedActions.Count == 0)
			{
				IsFullyExpanded = true;
				return this;
			}

			// pick random untried action (helps exploration)
			int index = random.Next(_untriedActions.Count);
			var actionPair = _untriedActions[index];
			_untriedActions.RemoveAt(index);

			if (_untriedActions.Count == 0)
				IsFullyExpanded = true;

			// clone current state
			var newState = simState.LightClone();

			// clone action + context for new state
			var clonedCtx = MonteCarloAgent.CloneContextFor(newState, actionPair.Item2);
			var clonedAct = MonteCarloAgent.CloneActionFor(newState, actionPair.Item1);

			engine.Resolve(newState, clonedCtx, clonedAct);

			// create child node representing RESULTING STATE
			var child = new MonteCarloNode()
			{
				Parent = this,
				engine = engine,
				simState = newState,
				clonedAction = clonedAct,
				clonedContext = clonedCtx,
				simPlayer = (CardBattleEngine.Player)newState.GetEntityById(simPlayer.Id),
				random = random,
				gameState_Hash_Score_Cache = gameState_Hash_Score_Cache,
			};

			Children.Add(child);
			return child;
		}
	}
}
