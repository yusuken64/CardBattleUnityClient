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
	public static GameStartParams GameStartParams;

	public Player Player;
	public Player Opponent;

	// Which CardBattleEngine.Player is "me", resolved once InitializeGame runs.
	public Guid? LocalPlayerId { get; private set; }

	public StartGameArgs Args { get; private set; }

	public AnimationQueue AnimationQueue;
	public DeckDefinition TestDeck;
	public DeckDefinition EnemyTestDeck;

	public GameEngine _engine { get; private set; }

	public IGameAgent _playerAgent;
	public IGameAgent _opponentAgent;
	public GameState _gameState { get; private set; }
	public bool ActivePlayerTurn { get; internal set; } //based on client animation timing 
	public bool OpponentTurn { get; internal set; }

	public bool UseSeed;
	public int RandomSeed;
	public BattleIntro BattleIntro;

	public string ServerUrl = "http://localhost:5299";

	private MiniSignalRClient _networkClient;
	private PlayerGameView _lastNetworkView;
	public PlayerGameView LastNetworkView => _lastNetworkView;
	private Guid _networkMatchId;
	private GameState _snapshotForDebug;

	public Action<GameEngine> GameInitialized;

	public AudioClip BattleMusic;

	void Start()
	{
		AudioManager.Instance.PlayMusic(BattleMusic);
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
		FindFirstObjectByType<ScrollingBackground>(FindObjectsInactive.Include)?
			.ActivateBackgroundByName(GameStartParams?.BackgroundName);
		
		_engine = new GameEngine();

		StartGameArgs args = PendingStartArgs ?? StartGameArgs.LocalTestDefault();
		Args = args;
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
		if (GameStartParams?.CombatDeck != null)
		{
			deck = GameStartParams.CombatDeck;
			GameStartParams.CombatDeck = null;
		}
		else
		{
			deck = TestDeck.ToDeck();
		}

		Deck enemyDeck;
		if (GameStartParams?.CombatDeckEnemy != null &&
			GameStartParams.CombatDeckEnemy.Cards.Any())
		{
			enemyDeck = GameStartParams.CombatDeckEnemy;
			GameStartParams.CombatDeckEnemy = null;
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

		SystemRNG rng = new SystemRNG();

		_gameState = CreateGame(deck, enemyDeck, rng);

		bool localIsP1 = args.LocalSeat == 0;
		int localIdx = localIsP1 ? 0 : 1;
		int remoteIdx = localIsP1 ? 1 : 0;

		if (GameStartParams != null)
		{
			_gameState.Players[localIdx].MaxHealth = GameStartParams.Health;
			_gameState.Players[localIdx].Health = GameStartParams.Health;
			_gameState.Players[remoteIdx].MaxHealth = GameStartParams.OpponentHealth;
			_gameState.Players[remoteIdx].Health = GameStartParams.OpponentHealth;
			if (GameStartParams.OpponentExtraEffects != null)
			{
				foreach (var opponentEffect in GameStartParams.OpponentExtraEffects)
				{
					_gameState.Players[remoteIdx].TriggeredEffects.Add(opponentEffect.CreateEffect());
				}
			}

			if (GameStartParams.AutoPlayer)
			{
				IGameAgent agent;
				if (GameStartParams.PlayerAgent == null)
				{
					agent = new AdvancedAI(_gameState.Players[localIdx], new SystemRNG());
				}
				else
				{
					agent = GameStartParams.PlayerAgent;
					agent.SetPlayer(_gameState.Players[localIdx]);
				}
				_playerAgent = agent;
				FindFirstObjectByType<GameResultScreen>(FindObjectsInactive.Include)
					.SetAutoAdvance(true);
			}
		}

		CardBattleEngine.Player localData = _gameState.Players[localIdx];
		CardBattleEngine.Player remoteData = _gameState.Players[remoteIdx];
		Deck localDeck = localIsP1 ? deck : enemyDeck;
		Deck remoteDeck = localIsP1 ? enemyDeck : deck;
		LocalPlayerId = localData.Id;

		SetupPlayer(localDeck, Player, localData);
		SetupPlayer(remoteDeck, Opponent, remoteData);

		//_opponentAgent = new BasicAI(Opponent.Data, rng);
		_opponentAgent = new AdvancedAI(Opponent.Data, rng);
		//_opponentAgent = new RandomAI(Opponent.Data, rng);

		_engine.ActionPlaybackCallback += ActionPlaybackCallback;
		_engine.ActionResolvedCallback += ActionResolvedCallback;

		Player.HeroPortrait.gameObject.SetActive(false);
		Opponent.HeroPortrait.gameObject.SetActive(false);
		BattleIntro.Setup(localDeck, remoteDeck);
		BattleIntro.gameObject.SetActive(true);
		BattleIntro.DoIntro(() =>
		{
			Player.HeroPortrait.gameObject.SetActive(true);
			Opponent.HeroPortrait.gameObject.SetActive(true);

			StartCoroutine(WaitToStartRoutine());
		});

		GameInitialized?.Invoke(this._engine);
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
		_networkClient.On<Guid>("OnMatchFound", matchId => Debug.Log($"Match found: {matchId}"));

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

	// Pull-based state fetch for the caller's own seat. Not called anywhere yet - added so the hub's
	// full RPC surface is available once a rendering adapter or reconnect flow needs it.
	private async Task<PlayerGameView> GetStateAsync(Guid matchId)
	{
		return await _networkClient.InvokeAsync<PlayerGameView>("GetState", matchId);
	}

	// Manual reconnect/resync fallback - pulls the server's latest known state for this match
	// and snaps the board to match it. Not wired to auto-fire on disconnect (there's no
	// reconnect-detection in MiniSignalRClient yet) - call this from a debug/dev menu.
	public async Task ResyncFromServer()
	{
		if (_networkClient == null)
		{
			Debug.LogError("No active network connection to resync from.");
			return;
		}

		PlayerGameView view = await GetStateAsync(_networkMatchId);
		ApplyBoardState(BoardStateSnapshot.FromPlayerGameView(view));
	}

	// Instant snap (no animation) of both players' hand/board/health/mana to match snapshot.
	// Not part of the normal per-action AnimationQueue rendering path - only for resync
	// fallback and the debug ContextMenu pair below.
	public void ApplyBoardState(BoardStateSnapshot snapshot)
	{
		Player.Clear();
		Opponent.Clear();

		ApplyPlayerBoardState(Player, snapshot.Self, snapshot.IsFromNetwork);
		ApplyPlayerBoardState(Opponent, snapshot.Opponent, snapshot.IsFromNetwork);

		LocalPlayerId = snapshot.LocalPlayerId;
	}

	private void ApplyPlayerBoardState(Player player, PlayerBoardSnapshot snapshot, bool isFromNetwork)
	{
		player.Data = snapshot.Data;

		var cardPrefab = FindFirstObjectByType<GameInteractionHandler>().CardPrefab;
		foreach (var cardData in snapshot.Data.Hand)
		{
			var newCard = Instantiate(cardPrefab, player.Hand.transform);
			newCard.Setup(cardData);
			player.Hand.AddCard(newCard);
		}

		for (int i = 0; i < snapshot.HiddenHandCount; i++)
		{
			// Opponent hand cards whose identity is unknown (hidden by the server by design) -
			// instantiate the prefab but skip Setup() entirely; FlippableCard defaults to its
			// back-facing state, so no CardBattleEngine.Card object is needed for a pure
			// count-only placeholder.
			var placeholderCard = Instantiate(cardPrefab, player.Hand.transform);
			player.Hand.AddCard(placeholderCard);
		}

		var minionPrefab = FindFirstObjectByType<GameInteractionHandler>().MinionPrefab;
		for (int i = 0; i < snapshot.Data.Board.Count; i++)
		{
			var minionData = snapshot.Data.Board[i];
			var newMinion = Instantiate(minionPrefab, player.Board.transform);
			newMinion.Setup(minionData);
			player.Board.Minions.Add(newMinion);

			if (isFromNetwork && snapshot.SourceMinionViews != null && i < snapshot.SourceMinionViews.Count)
			{
				// HasDeathRattle/HasTrigger can't be trusted from the reconstructed
				// CardBattleEngine.Minion (its TriggeredEffects only reflect the base card,
				// not any ability granted/removed at runtime) - the server-computed values on
				// the view are authoritative, so they overwrite whatever Setup() just derived.
				var mv = snapshot.SourceMinionViews[i];
				newMinion.HasDeathRattle = mv.HasDeathRattle;
				newMinion.HasTrigger = mv.HasTrigger;
				newMinion.UpdateUI();
			}
		}

		player.Board.UpdateMinionPositions();
		player.RefreshData();

		// Snap instantly instead of letting Card/Minion.Update() lerp toward TargetPosition.
		foreach (var card in player.Hand.Cards)
		{
			card.Moving = false;
			card.transform.localPosition = new Vector3(card.TargetPosition.x, card.TargetPosition.y, 0);
		}
		foreach (var minion in player.Board.Minions)
		{
			minion.Moving = false;
			minion.transform.localPosition = new Vector3(minion.TargetPosition.x, minion.TargetPosition.y, 0);
		}
	}

	// Matchmaking queue entry points. Not called anywhere yet - there is no lobby/matchmaking UI in
	// this project; added so the hub's full RPC surface is available for whenever that UI exists.
	private async Task JoinQueueAsync(DecklistRequest deck)
	{
		await _networkClient.InvokeAsync<object>("JoinQueue", deck);
	}

	private async Task LeaveQueueAsync()
	{
		await _networkClient.InvokeAsync<object>("LeaveQueue");
	}

	private void SetupPlayer(Deck deck, Player player, CardBattleEngine.Player data)
	{
		player.Data = data;
		player.HeroImage.sprite = deck.HeroCard.Sprite;
		player.HeroPower.OriginalCard = deck.HeroCard.CreateCard();
		player.HeroPower.Data = player.Data.HeroPower;
		player.RefreshData();
	}

	private GameState CreateGame(Deck playerDeck, Deck enemyDeck, IRNG rng)
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

			var first = allMinions.FirstOrDefault(x => x?.Data?.Id == minion.Id);
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

			var first = allCards.FirstOrDefault(x => x?.Data?.Id == card.Id);
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
				Debug.LogError(action);
				Debug.LogError(exception);
			}
		}
	}

	internal Player GetPlayerFor(CardBattleEngine.Player sourcePlayer)
	{
		return Player.Data.Id == sourcePlayer?.Id ? Player : Opponent;
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
		IGameAgent agent = null;
		if (!OpponentTurn &&
			_gameState.CurrentPlayer.Id == Player.Data.Id &&
			_playerAgent != null)
		{
			agent = _playerAgent;
		}
		else if (
			OpponentTurn &&
			_gameState.CurrentPlayer.Id == Opponent.Data.Id)
		{
			agent = _opponentAgent;
		}

		_ = ProcessMoveAsync(agent);

		Opponent.UpdatePlayableActions(false);
		Player.UpdatePlayableActions(
			ActivePlayerTurn &&
			state.CurrentPlayer == Player.Data);
	}

	public async Task ProcessMoveAsync(IGameAgent gameAgent)
	{
		if (gameAgent == null)
			return;

		var nextAction = await Task.Run(() =>
			gameAgent.GetNextAction(_gameState));

		if (TryResolveAction(nextAction))
			return;

		Debug.LogError("Invalid action, retrying...");
		await Task.Yield();
		await ProcessMoveAsync(gameAgent);
	}

	private bool TryResolveAction((IGameAction action, ActionContext context) nextAction)
	{
		if (nextAction.action.IsValid(_gameState, nextAction.context, out string reason))
		{
			ResolveAction(nextAction.action, nextAction.context);
			return true;
		}

		Debug.LogError($"Invalid Action: {reason}");
		return false;
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
			//CardBattleEngine.Minion minionData = data.Board[i];
			//var boardMinion = minions[i];

			//AssertAreEqual(boardMinion.Data.Id, minionData.Id);
			//AssertAreEqual(boardMinion.Attack, minionData.Attack);
			//AssertAreEqual(boardMinion.Health, minionData.Health);
			//AssertAreEqual(boardMinion.CanAttack, minionData.CanAttack());
		}
	}

	private static void AssertAreEqual<T>(T a, T b)
	{
		if (!EqualityComparer<T>.Default.Equals(a, b))
		{
			throw new Exception($"Validation exception: {a} != {b}");
		}
	}

	[ContextMenu("Snapshot GameState")]
	private void SnapshotGameStateForDebug()
	{
		_snapshotForDebug = _gameState.Clone();
		Debug.Log("GameState snapshot captured.");
	}

	[ContextMenu("Restore GameState")]
	private void RestoreGameStateForDebug()
	{
		if (_snapshotForDebug == null)
		{
			Debug.LogError("No snapshot captured yet - use \"Snapshot GameState\" first.");
			return;
		}

		ApplyBoardState(BoardStateSnapshot.FromGameState(_snapshotForDebug, LocalPlayerId.Value));
	}

	// Writes the current board state (both players' full hand/board/hero stats) to a JSON file -
	// callable from the Board State editor window, or any other future debug/dev entry point.
	public void ExportBoardStateToFile(string filePath)
	{
		if (!LocalPlayerId.HasValue)
		{
			Debug.LogError("No LocalPlayerId set - cannot export board state.");
			return;
		}

		BoardStateFile.Export(_gameState, LocalPlayerId.Value, filePath);
		Debug.Log($"Board state exported to {filePath}");
	}

	// Reads a JSON file written by ExportBoardStateToFile and snaps the board to match it.
	public void ImportBoardStateFromFile(string filePath)
	{
		BoardStateSnapshot snapshot = BoardStateFile.Import(filePath);
		ApplyBoardState(snapshot);
		Debug.Log($"Board state imported from {filePath}");
	}
}

public class GameStartParams
{
	public bool BlockStart = false;	//the game will not start until this is true
	public bool SkipMulligan = false;
	public bool SkipShuffle = false;
	public int InitialCards = 3;

	public List<TriggeredEffectWrapper> OpponentExtraEffects;

	public Deck CombatDeck;
	public int Health = 30;
	public Deck CombatDeckEnemy;
	public int OpponentHealth = 30;

	public string BackgroundName;

	public bool AutoPlayer; //assign ai to player
	public IGameAgent PlayerAgent; //null to use default
}

public class UnityRNG : IRNG
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
public class SystemRNG : IRNG
{
	private readonly System.Random _rng;

	// Optional: allow providing a seed
	public SystemRNG(int? seed = null)
	{
		_rng = seed.HasValue ? new System.Random(seed.Value) : new System.Random();
	}

	// Clone creates a new RNG with a "randomized" seed based on current RNG state
	public IRNG Clone()
	{
		// Generate a seed from the current RNG (so clone has independent sequence)
		int newSeed = _rng.Next();
		return new SystemRNG(newSeed);
	}

	public double NextDouble()
	{
		// Returns a double in [0,1)
		return _rng.NextDouble();
	}

	public int NextInt(int maxExclusive)
	{
		return _rng.Next(maxExclusive); // [0, maxExclusive)
	}

	public int NextInt(int minInclusive, int maxExclusive)
	{
		return _rng.Next(minInclusive, maxExclusive); // [minInclusive, maxExclusive)
	}
}
