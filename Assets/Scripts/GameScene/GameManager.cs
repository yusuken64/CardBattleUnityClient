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
	public static GameStartParams GameStartParams;

	public Player Player;
	public Player Opponent;

	public AnimationQueue AnimationQueue;
	public DeckDefinition TestDeck;
	public DeckDefinition EnemyTestDeck;

	public GameEngine _engine { get; private set; }

	private IGameAgent _opponentAgent;
	public GameState _gameState { get; private set; }
	public bool ActivePlayerTurn { get; internal set; } //based on client animation timing 
	public bool OpponentTurn { get; internal set; }

	public bool UseSeed;
	public int RandomSeed;
	public BattleIntro BattleIntro;

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
			if (EnemyTestDeck != null)
			{
				enemyDeck = EnemyTestDeck.ToDeck();
			}
			else
			{
				enemyDeck = TestDeck.ToDeck();
			}
		}

		int seed = 0;
#if UNITY_EDITOR
		if (UseSeed)
		{
			UnityEngine.Random.InitState(seed);
		}
#endif

		UnityRNG rng = new UnityRNG();

		_gameState = CreateGame(deck, enemyDeck, rng);
		if (GameStartParams != null &&
			GameStartParams.OpponentExtraEffects != null)
		{
			foreach (var opponentEffect in GameStartParams.OpponentExtraEffects)
			{
				_gameState.Players[1].TriggeredEffects.Add(opponentEffect.CreateEffect());
			}
		}

		SetupPlayer(deck, Player, _gameState.Players[0]);
		SetupPlayer(enemyDeck, Opponent, _gameState.Players[1]);

		_opponentAgent = new BasicAI(Opponent.Data, rng);
		//_opponentAgent = new RandomAI(Opponent.Data, rng);

		_engine.ActionPlaybackCallback += ActionPlaybackCallback;
		_engine.ActionResolvedCallback += ActionResolvedCallback;

		Player.HeroPortrait.gameObject.SetActive(false);
		Opponent.HeroPortrait.gameObject.SetActive(false);
		BattleIntro.Setup(deck, enemyDeck);
		BattleIntro.gameObject.SetActive(true);
		BattleIntro.DoIntro(() =>
		{
			Player.HeroPortrait.gameObject.SetActive(true);
			Opponent.HeroPortrait.gameObject.SetActive(true);

			StartCoroutine(WaitToStartRoutine());
		});
	}

	private IEnumerator WaitToStartRoutine()
	{
		while(GameStartParams != null &&
			  GameStartParams.BlockStart)
		{
			yield return null;
		}

		_engine.StartGame(_gameState);
		GameStartParams = null;
	}

	private void SetupPlayer(Deck deck, Player player, CardBattleEngine.Player data)
	{
		player.Data = data;
		player.HeroImage.sprite = deck.HeroCard.Sprite;
		player.HeroPower.OriginalCard = deck.HeroCard.CreateCard();
		player.HeroPower.Data = player.Data.HeroPower;
		player.RefreshData();
	}

	private GameState CreateGame(Deck playerDeck, Deck enemyDeck, UnityRNG rng)
	{
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

		var gameState =  new GameState(p1, p2, rng, cardDB);

		if (GameStartParams != null)
		{
			gameState.SkipShuffle = GameStartParams.SkipShuffle;
			gameState.SkipMulligan = GameStartParams.SkipMulligan;
			gameState.InitialCards = GameStartParams.InitialCards;
		}

		return gameState;
	}

	internal GameObject GetObjectFor(IGameEntity entity)
	{
		if (entity == null) { return null; }

#pragma warning disable CS0252 // Possible unintended reference comparison; left hand side needs cast
		if (entity.Id == Player.Data.Id) { return Player.HeroPortrait.gameObject; }
		if (entity.Id == Opponent.Data.Id) { return Opponent.HeroPortrait.gameObject; }
#pragma warning restore CS0252 // Possible unintended reference comparison; left hand side needs cast

		if (entity is CardBattleEngine.Minion minion)
		{
			var allMinions = new List<Minion>();
			allMinions.AddRange(Player.Board.Minions);
			allMinions.AddRange(Opponent.Board.Minions);

			var first = allMinions.FirstOrDefault(x => x.Data.Id == minion.Id);
			if (first != null)
			{
				return first.gameObject;
			}
		}

		if (entity is CardBattleEngine.Card card)
		{
			var allCards = new List<Card>();
			allCards.AddRange(Player.Hand.Cards);
			allCards.AddRange(Opponent.Hand.Cards);

			var first = allCards.FirstOrDefault(x => x.Data.Id == card.Id);
			if (first != null)
			{
				return first.gameObject;
			}
		}

		//Debug.LogError($"Source for {entity} {entity.Id} invalid");
		return null;
	}

	internal IEnumerable<IUnityGameEntity> EnumerateAllEntities()
	{
		yield return Player;
		foreach (var c in Player.Hand.Cards) yield return c;
		foreach (var m in Player.Board.Minions) yield return m;

		yield return Opponent;
		foreach (var c in Opponent.Hand.Cards) yield return c;
		foreach (var m in Opponent.Board.Minions) yield return m;
	}

	internal IUnityGameEntity GetObjectByID(Guid id)
	{
		return EnumerateAllEntities()
			.FirstOrDefault(x => x.Entity?.Id == id);
	}

	public bool CheckIsValid(IGameAction action, ActionContext context, out string reason)
	{
		if (!ActivePlayerTurn)
		{
			reason = "Not your Turn";
			return false;
		}

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
		return Player.Data.Id == sourcePlayer.Id ? Player : Opponent;
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		//Debug.Log(current);
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
		ProcessEnemyMove();

		Opponent.UpdatePlayableActions(false);
		Player.UpdatePlayableActions(
			ActivePlayerTurn &&
			state.CurrentPlayer == Player.Data);
	}

	public void ProcessEnemyMove()
	{
		if (OpponentTurn &&
			_gameState.CurrentPlayer.Id == Opponent.Data.Id)
		{
			(IGameAction, ActionContext) nextAction = ((IGameAgent)_opponentAgent).GetNextAction(_gameState);
			((IGameAgent)_opponentAgent).SetTarget(nextAction, _gameState);

			string actionString = nextAction.Item1.ToString();
			IGameAction item1 = nextAction.Item1;
			if (item1 is PlayCardAction playCardAction)
			{
				actionString = $"Play card {playCardAction.Card.Name}";
			}
			else if (item1 is AttackAction attackAction)
			{
				var attackSource = nextAction.Item2.Source;
				var attackTarget = nextAction.Item2.Target;
				actionString = $"Attack {attackSource} to {attackTarget}";
			}
			Debug.Log($"Enemy Action {actionString}");
			ResolveAction(nextAction.Item1, nextAction.Item2);
		}
	}

	public static void ValidateState(CardBattleEngine.Player data, Player player)
	{
		if (player.Board.Minions.Any(x => x == null))
		{
			throw new Exception($"null minion");
		}

		var minions = player.Board.Minions
						.Where(x => x)
						.Where(x => x.Data != null).ToList();
		if (minions.Count() != data.Board.Count())
		{
			Debug.Log("========Board minions");
			foreach (var minion in minions)
			{
				Debug.Log(minion.Data);
			}

			Debug.Log("========Data minions");
			foreach (var minion in data.Board)
			{
				Debug.Log(minion);
			}

			Debug.LogError("Minion count mismatch");
			return;
		}

		for (int i = 0; i < data.Board.Count; i++)
		{
			CardBattleEngine.Minion minionData = data.Board[i];
			var boardMinion = minions[i];

			AssertAreEqual(boardMinion.Data.Id, minionData.Id);
			AssertAreEqual(boardMinion.Attack, minionData.Attack);
			AssertAreEqual(boardMinion.Health, minionData.Health);
			AssertAreEqual(boardMinion.CanAttack, minionData.CanAttack());
		}
	}

	private static void AssertAreEqual<T>(T a, T b)
	{
		if (!EqualityComparer<T>.Default.Equals(a, b))
		{
			throw new Exception($"Validation exception: {a} != {b}");
		}
	}
}

public class GameStartParams
{
	public bool BlockStart = false;	//the game will not start until this is true
	public bool SkipMulligan = false;
	public bool SkipShuffle = false;
	public int InitialCards = 3;

	public List<TriggeredEffectWrapper> OpponentExtraEffects;
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