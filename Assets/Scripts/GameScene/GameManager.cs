using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static string ReturnScreenName;
	public static Func<bool, IEnumerator> GameResultRoutine;
	public Player Player;
	public Player Opponent;

	public AnimationQueue AnimationQueue;
	public DeckDefinition TestDeck;

	public GameEngine _engine { get; private set; }

	private RandomAI _opponentAgent;
	public GameState _gameState { get; private set; }
	public bool ActivePlayerTurn { get; internal set; } //based on client animation timing 
	public bool OpponentTurn { get; internal set; }

	public bool UseSeed;
	public int RandomSeed;

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
		_engine = new GameEngine();

		Deck deck;
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
		if (gameSaveData.CombatDeck != null)
		{
			deck = gameSaveData.CombatDeck.ToDeck();
			gameSaveData.CombatDeck = null;
		}
		else
		{
			deck = TestDeck.ToDeck();
		}

		Deck enemyDeck;
		if (gameSaveData.CombatDeckEnemy != null &&
			gameSaveData.CombatDeckEnemy.CardIDs.Any())
		{
			enemyDeck = gameSaveData.CombatDeckEnemy.ToDeck();
			gameSaveData.CombatDeckEnemy = null;
		}
		else
		{
			enemyDeck = TestDeck.ToDeck();
		}

		int seed = 0;
		if (UseSeed)
		{
			UnityEngine.Random.InitState(seed);
		}

		UnityRNG rng = new UnityRNG();

		_gameState = CreateTestGame(deck, enemyDeck, rng);
		SetupPlayer(deck, Player, _gameState.Players[0]);
		SetupPlayer(enemyDeck, Opponent, _gameState.Players[1]);

		_opponentAgent = new RandomAI(Opponent.Data, rng);

		_engine.ActionPlaybackCallback = ActionPlaybackCallback;
		_engine.ActionResolvedCallback = ActionResolvedCallback;
		_engine.StartGame(_gameState);
	}

	private void SetupPlayer(Deck deck, Player player, CardBattleEngine.Player data)
	{
		player.Data = data;
		player.HeroImage.sprite = deck.HeroCard.Sprite;
		player.HeroPower.OriginalCard = deck.HeroCard.CreateCard();
		player.HeroPower.Data = player.Data.HeroPower;
		player.RefreshData();
	}

	private GameState CreateTestGame(Deck playerDeck, Deck enemyDeck, UnityRNG rng)
	{
		var cardManager = Common.Instance.CardManager;

		CardBattleEngine.Player p1 = new CardBattleEngine.Player("Alice");
		p1.Deck.AddRange(playerDeck.Cards.Select(x => x.CreateCard()).ToList());
		p1.Deck.ForEach(x => x.Owner = p1);
		p1.HeroPower = HeroPowerDefinition.CreateHeroPowerFromHeroCard(playerDeck.HeroCard as MinionCardDefinition);

		CardBattleEngine.Player p2 = new CardBattleEngine.Player("Bob");
		p2.Deck.AddRange(enemyDeck.Cards.Select(x => x.CreateCard()).ToList());
		p2.Deck.ForEach(x => x.Owner = p2);
		p2.HeroPower = HeroPowerDefinition.CreateHeroPowerFromHeroCard(enemyDeck.HeroCard as MinionCardDefinition);

		List<CardBattleEngine.Card> cardDB = Common.Instance.CardManager.AllCards()
			.Select(x => x.CreateCard()).ToList();
		return new GameState(p1, p2, rng, cardDB);
	}

	internal GameObject GetObjectFor(IGameEntity source)
	{
#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
		if (source == Player.Data) { return Player.HeroPortrait.gameObject; }
		if (source == Opponent.Data) { return Opponent.HeroPortrait.gameObject; }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast

		if (source is CardBattleEngine.Minion minion)
		{
			var allMinions = new List<Minion>();
			allMinions.AddRange(Player.Board.Minions);
			allMinions.AddRange(Opponent.Board.Minions);

			var first = allMinions.FirstOrDefault(x => x.Data == minion);
			if (first != null)
			{
				return first.gameObject;
			}
		}

		return null;
	}

	public bool CheckIsValid(IGameAction action, ActionContext context, out string reason)
	{
		return action.IsValid(_gameState, context, out reason);
	}

	public void ResolveAction(IGameAction action, ActionContext context)
	{
		if (action.IsValid(_gameState, context, out string reason))
		{
			try
			{
				_engine.Resolve(_gameState, context, action);
			}
			catch (System.Exception exception)
			{
				Debug.LogError(reason);
				Debug.LogError(exception);
			}
		}
	}

	internal Player GetPlayerFor(CardBattleEngine.Player sourcePlayer)
	{
		return Player.Data == sourcePlayer ? Player : Opponent;
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		Debug.Log(current);
		AnimationQueue.EnqueueAnimation(this, state, current);

		if (state.CurrentPlayer == Player.Data)
		{
			var validActions = state.GetValidActions(Player.Data)
				.ToList();

			if (validActions.Count() == 1 &&
				validActions[0].Item1 is EndTurnAction)
			{
				//highlight endturn button;
				if (ActivePlayerTurn)
				{
					FindFirstObjectByType<UI>().EndTurnButton.SetToOnlyAction();
				}
			}
		}
	}

	private void ActionResolvedCallback(GameState state)
	{
		ProcessEnemyMove(state);

		Opponent.UpdatePlayableActions(false);
		Player.UpdatePlayableActions(
			ActivePlayerTurn &&
			state.CurrentPlayer == Player.Data);
	}

	public void ProcessEnemyMove(GameState state)
	{
		if (OpponentTurn &&
			state.CurrentPlayer == Opponent.Data)
		{
			(IGameAction, ActionContext) nextAction = ((IGameAgent)_opponentAgent).GetNextAction(state);
			((IGameAgent)_opponentAgent).SetTarget(nextAction, (x) =>
			{
				var triggerSource = nextAction.Item2?.Source as ITriggerSource;

				if (triggerSource == null) { return null; }
				var targets = state.GetValidTargets(triggerSource, x);

				if (!targets.Any()) { return null; }

				return targets[UnityEngine.Random.Range(0, targets.Count())];
			});

			string actionString = nextAction.Item1.ToString();
			IGameAction item1 = nextAction.Item1;
			if (item1 is PlayCardAction playCardAction)
			{
				actionString = $"Play card {playCardAction.Card.Name}";
			}
			Debug.Log($"Enemy Action {actionString}");
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