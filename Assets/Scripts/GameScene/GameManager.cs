using CardBattleEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public static string ReturnScreenName;
	public static Func<bool, IEnumerator> GameResultRoutine;

	// Set before SceneManager.LoadScene("GameScene") to configure this match. Null preserves legacy local single-player behavior.
	public static StartGameArgs PendingStartArgs;

	public Player Player;
	public Player Opponent;

	// Which CardBattleEngine.Player is "me", resolved once InitializeGame runs.
	public Guid? LocalPlayerId { get; private set; }

	public AnimationQueue AnimationQueue;
	public DeckDefinition TestDeck;

	public GameEngine _engine { get; private set; }

	private RandomAI _opponentAgent;
	public GameState _gameState { get; private set; }
	public bool ActivePlayerTurn { get; internal set; } //based on client animation timing 
	public bool OpponentTurn { get; internal set; }

	public bool UseSeed;
	public int RandomSeed;
	public BattleIntro BattleIntro;

	public string ServerUrl = "http://localhost:5299";

	private MiniSignalRClient _networkClient;
	private PlayerGameView _lastNetworkView;
	private Guid _networkMatchId;

	void Start()
	{
		ClearBoard();
		InitializeGame();
	}

	void Update()
	{
		_networkClient?.PumpMainThread();
	}

	void OnDestroy()
	{
		if (_networkClient != null)
		{
			_ = _networkClient.DisconnectAsync();
		}
	}

	private void ClearBoard()
	{
		Player.Clear();
		Opponent.Clear();
	}

	private void InitializeGame()
	{
		_engine = new GameEngine();

		StartGameArgs args = PendingStartArgs ?? StartGameArgs.LocalTestDefault();
		PendingStartArgs = null;

		if (args.Mode == GameMode.Networked)
		{
			StartCoroutine(InitializeNetworkedGame(args));
		}
		else
		{
			InitializeLocalTestGame(args);
		}
	}

	private void InitializeLocalTestGame(StartGameArgs args)
	{
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
#if UNITY_EDITOR
		if (UseSeed)
		{
			UnityEngine.Random.InitState(seed);
		}
#endif

		UnityRNG rng = new UnityRNG();

		_gameState = CreateTestGame(deck, enemyDeck, rng);

		bool localIsP1 = args.LocalSeat == 0;
		CardBattleEngine.Player localData = localIsP1 ? _gameState.Players[0] : _gameState.Players[1];
		CardBattleEngine.Player remoteData = localIsP1 ? _gameState.Players[1] : _gameState.Players[0];
		Deck localDeck = localIsP1 ? deck : enemyDeck;
		Deck remoteDeck = localIsP1 ? enemyDeck : deck;
		LocalPlayerId = localData.Id;

		SetupPlayer(localDeck, Player, localData);
		SetupPlayer(remoteDeck, Opponent, remoteData);

		_opponentAgent = new RandomAI(Opponent.Data, rng);

		_engine.ActionPlaybackCallback = ActionPlaybackCallback;
		_engine.ActionResolvedCallback = ActionResolvedCallback;

		Player.HeroPortrait.gameObject.SetActive(false);
		Opponent.HeroPortrait.gameObject.SetActive(false);
		BattleIntro.Setup(localDeck, remoteDeck);
		BattleIntro.gameObject.SetActive(true);
		BattleIntro.DoIntro(() =>
		{
			Player.HeroPortrait.gameObject.SetActive(true);
			Opponent.HeroPortrait.gameObject.SetActive(true);
			_engine.StartGame(_gameState);
		});
	}

	private IEnumerator InitializeNetworkedGame(StartGameArgs args)
	{
		Task initTask = InitializeNetworkedGameAsync(args);
		while (!initTask.IsCompleted)
		{
			yield return null;
		}

		if (initTask.IsFaulted)
		{
			Debug.LogError($"Networked game initialization failed: {initTask.Exception}");
		}
	}

	// Connects to GameServer's MatchHub and either creates or joins a match. Note this only stands
	// up the connection/submission channel - Player/Opponent/Board still expect live CardBattleEngine
	// objects (see InitializeLocalTestGame), and nothing here renders from the PlayerGameView pushes
	// yet. That's a separate rendering adapter, not part of this.
	private async Task InitializeNetworkedGameAsync(StartGameArgs args)
	{
		_networkClient = new MiniSignalRClient($"{ServerUrl}/hubs/match");
		_networkClient.On<PlayerGameView>("OnStateUpdated", OnNetworkStateUpdated);
		_networkClient.On<string>("OnActionRejected", reason => Debug.LogWarning($"Action rejected: {reason}"));
		_networkClient.On<Guid?>("OnMatchEnded", winnerId => Debug.Log($"Networked match ended. Winner: {winnerId}"));

		await _networkClient.ConnectAsync();

		bool isHost = string.IsNullOrEmpty(args.MatchId);
		DecklistRequest decklist = TestDeck.ToDeck().ToDecklistRequest(isHost ? "Host" : "Joiner");

		if (isHost)
		{
			_networkMatchId = await _networkClient.InvokeAsync<Guid>("CreateMatch", decklist);
			Debug.Log($"Created match {_networkMatchId}. Waiting for an opponent to join.");
		}
		else
		{
			_networkMatchId = Guid.Parse(args.MatchId);
			JoinResult joinResult = await _networkClient.InvokeAsync<JoinResult>("JoinMatch", _networkMatchId, decklist);
			if (!joinResult.Success)
			{
				Debug.LogError($"Failed to join match {_networkMatchId}: {joinResult.Error}");
			}
		}
	}

	private void OnNetworkStateUpdated(PlayerGameView view)
	{
		_lastNetworkView = view;

		if (LocalPlayerId == null)
		{
			LocalPlayerId = view.ViewerPlayerId;
		}

		if (view.IsGameOver)
		{
			Debug.Log($"Networked match over. Winner: {view.WinnerPlayerId}");
		}
	}

	// Submits one of the options offered in _lastNetworkView.LegalActions back to the server.
	// Matching a board click (which card/minion was targeted) to the right LegalActionView is the
	// rendering adapter's job, not this method's - callers just hand over the chosen entry.
	public void SubmitLocalAction(LegalActionView chosen)
	{
		if (_networkClient == null || _lastNetworkView?.PromptVersion == null)
		{
			Debug.LogError("No active network prompt to submit an action against.");
			return;
		}

		_ = SubmitLocalActionAsync(chosen, _networkMatchId, _lastNetworkView.PromptVersion.Value);
	}

	private async Task SubmitLocalActionAsync(LegalActionView chosen, Guid matchId, int promptVersion)
	{
		ActionResult result = await _networkClient.InvokeAsync<ActionResult>("SubmitAction", matchId, chosen.Index, promptVersion);
		if (!result.Success)
		{
			Debug.LogWarning($"Server rejected action: {result.Error}");
		}
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
			((IGameAgent)_opponentAgent).SetTarget(nextAction, (x) =>
			{
				var triggerSource = nextAction.Item2?.Source as ITriggerSource;

				if (triggerSource == null) { return null; }
				var targets = _gameState.GetValidTargets(triggerSource, x);

				if (!targets.Any()) { return null; }

				if (targets.Contains(triggerSource))
				{
					targets.Remove(triggerSource);
				}

				return targets[UnityEngine.Random.Range(0, targets.Count())];
			});

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