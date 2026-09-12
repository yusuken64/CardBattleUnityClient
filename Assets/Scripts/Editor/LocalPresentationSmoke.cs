using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using CardBattleEngine;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Runs the real scene/prefabs against a tiny deterministic local game without editing card or scene assets.
[InitializeOnLoad]
public static class LocalPresentationSmoke
{
    private const string Key = "MonsterGirl.LocalPresentationSmoke";
    private static double _deadline;
    private static bool _attached;
    private static bool _failed;
    private static GameState _networkState;
    private static GameEngine _networkEngine;
    private static Action<PlayerGameView> _receive;
    private static readonly List<CardBattleEngine.View.PlaybackEventView> Frames = new();
    private static long _sequence;
    private static long _revision;
    private static int _viewerSeat;
    private static MinionCardDefinition _pendingNetworkToken;
    private static MinionCardDefinition _leader;
    static LocalPresentationSmoke()
    {
        EditorApplication.update += Tick;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.EnteredEditMode && SessionState.GetBool(Key, false))
            {
                SessionState.SetBool(Key, false);
                var previous = SessionState.GetString(Key + ".Scene", "");
                if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
            }
        };
    }

    [MenuItem("Tools/Network/Validate Local Presentation")]
    public static void Run()
        => Begin(-1);

    [MenuItem("Tools/Network/Validate Network Presentation Seat 1")]
    public static void RunNetworkFirst() => Begin(0);
    [MenuItem("Tools/Network/Validate Network Presentation Seat 2")]
    public static void RunNetworkSecond() => Begin(1);

    private static void Begin(int viewerSeat)
    {
        if (EditorApplication.isPlaying || SceneManager.GetActiveScene().isDirty)
            throw new InvalidOperationException("Run validation from edit mode with a saved scene.");
        SessionState.SetString(Key + ".Scene", SceneManager.GetActiveScene().path);
        SessionState.SetBool(Key, true);
        SessionState.SetInt(Key + ".Seat", viewerSeat);
        EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
        EditorApplication.isPlaying = true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Arm()
    {
        if (!SessionState.GetBool(Key, false)) return;
        Application.runInBackground = true;
        _deadline = EditorApplication.timeSinceStartup + 150;
        _failed = false;
        _attached = false;
        _networkState = null;
        _pendingNetworkToken = null;
        _viewerSeat = SessionState.GetInt(Key + ".Seat", -1);
        SceneManager.sceneLoaded += Configure;
        Application.logMessageReceived += OnLog;
    }

    private static void Configure(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "GameScene" || Common.Instance == null || Common.Instance.CardManager == null) return;
        SceneManager.sceneLoaded -= Configure;
        var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        var self = new Deck { Title = "Playback Self", HeroCard = gm.TestDeck.HeroCard };
        var opponent = new Deck { Title = "Playback Opponent", HeroCard = gm.TestDeck.HeroCard };
        var token = ScriptableObject.CreateInstance<MinionCardDefinition>();
        token.CardName = "Playback validation minion";
        token.Cost = 0; token.Attack = 2; token.Health = 2;
        token.ID = ((MinionCardDefinition)self.HeroCard).ID;
        _leader = ScriptableObject.CreateInstance<MinionCardDefinition>();
        _leader.ID = token.ID; _leader.CardName = "Playback leader"; _leader.Cost = 0;
        _leader.MinionTriggeredEffects.Add(new TriggeredEffectWrapper
        {
            EffectTrigger = EffectTrigger.Battlecry,
            AffectedEntitySelectorWrapper = new ContextSelectorWrapper { IncludeSummonedMinion = true },
            GameActions = new List<IGameActionWrapperBase>
            { new GainArmorActionWrapper { Amount = new ConstantValueWrapper { Number = 1 } } },
        });
        self.HeroCard = opponent.HeroCard = _leader;
        for (int i = 0; i < 4; i++) { self.Cards.Add(token); opponent.Cards.Add(token); }
        if (_viewerSeat >= 0)
        {
            gm.enabled = false;
            _pendingNetworkToken = token; // Common.Start must finish loading the card catalog first.
            return;
        }
        GameManager.PendingStartArgs = StartGameArgs.LocalTestDefault();
        GameManager.GameStartParams = new GameStartParams
        {
            CombatDeck = self, CombatDeckEnemy = opponent, Health = 8, OpponentHealth = 8,
            InitialCards = 2, SkipShuffle = true, SkipMulligan = false, AutoPlayer = true,
            PlayerAgent = new SmokeAgent(),
        };
        gm.GameInitialized += _ => gm._opponentAgent = new SmokeAgent();
    }

    private static void ConfigureNetwork(GameManager gm, MinionCardDefinition token)
    {
        gm.enabled = false; // Drive the network receiver directly; transport is tested by GameServer.Test.
        typeof(GameManager).GetProperty(nameof(GameManager.Args)).SetValue(gm, new StartGameArgs { Mode = GameMode.Networked });
        typeof(GameManager).GetField("_networkClient", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(gm, new MiniSignalRClient("http://127.0.0.1:1/hubs/match"));
        _receive = (Action<PlayerGameView>)Delegate.CreateDelegate(typeof(Action<PlayerGameView>), gm,
            typeof(GameManager).GetMethod("OnNetworkStateUpdated", BindingFlags.Instance | BindingFlags.NonPublic));
        var first = new CardBattleEngine.Player("First") { Health = 8, MaxHealth = 8 };
        var second = new CardBattleEngine.Player("Second") { Health = 8, MaxHealth = 8 };
        foreach (var player in new[] { first, second })
        {
            player.HeroPower = HeroPowerDefinition.CreateHeroPowerFromHeroCard(_leader);
            player.HeroPower.LeaderCard.Owner = player;
            if (_viewerSeat == 1)
                player.HeroPower.ValidTargetSelector = new EntityTypeSelector
                { EntityTypes = EntityType.Player, TeamRelationship = TeamRelationship.Friendly };
            for (int i = 0; i < 4; i++) { var card = token.CreateCard(); card.Owner = player; player.Deck.Add(card); }
        }
        _networkState = new GameState(first, second, new XorShiftRNG(1), first.Deck)
            { InitialCards = 2, SkipShuffle = true };
        _networkEngine = new GameEngine();
        _sequence = _revision = 0;
        Frames.Clear();
        _networkEngine.ActionPlaybackCallback = (state, current) => Frames.Add(CardBattleEngine.View.PlayerViewBuilder.BuildPlayback(
            state, state.Players[_viewerSeat], current.action, current.context, ++_sequence));
        PublishNetwork();
        _networkEngine.StartGame(_networkState);
        PublishNetwork();
    }

    private static void PublishNetwork()
    {
        var viewer = _networkState.Players[_viewerSeat];
        var next = _networkState.PendingChoice?.SourcePlayer ?? _networkState.CurrentPlayer;
        var options = _networkState.IsGameOver() || next != viewer ? new List<(IGameAction, ActionContext)>() :
            _networkState.PendingChoice != null ? _networkState.PendingChoice.GetActions(_networkState).ToList() : _networkState.GetValidActions(viewer);
        var view = CardBattleEngine.View.PlayerViewBuilder.Build(_networkState, viewer, canonicalLegalActions: options, promptVersion: options.Count > 0 ? (int)(_revision + 1) : (int?)null);
        view.StateRevision = ++_revision;
        view.PlaybackSequence = _sequence;
        view.PlaybackEvents = Frames.ToList();
        Frames.Clear();
        var wire = JObject.FromObject(view).ToObject<PlayerGameView>();
        _receive(wire);
        _receive(wire); // Redelivery must never replay the same batch.
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error) _failed = true;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || _deadline == 0) return;
        var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        if (_pendingNetworkToken != null && gm != null && Common.Instance?.CardManager != null &&
            typeof(CardManager).GetField("_cardLookup", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(Common.Instance.CardManager) != null)
        {
            var token = _pendingNetworkToken;
            _pendingNetworkToken = null;
            ConfigureNetwork(gm, token);
        }
        if (gm?.AnimationQueue != null && !_attached)
        {
            _attached = true;
            gm.AnimationQueue.PlaybackFailed += () => _failed = true;
        }
        var state = _networkState ?? gm?._gameState;
        bool completed = state != null && state.IsGameOver() && !gm.AnimationQueue.IsPlaying;
        if (_networkState != null && gm != null && !_failed && !completed && !gm.NetworkAnimationQueue.IsProcessing)
        {
            try
            {
                ValidateDisplayedPlayer(gm.Player, _networkState.Players[_viewerSeat], false);
                ValidateDisplayedPlayer(gm.Opponent, _networkState.Players[1 - _viewerSeat], true);
                var viewer = _networkState.Players[_viewerSeat];
                if (_networkState.CurrentPlayer == viewer && _networkState.PendingChoice == null &&
                    _networkState.GetValidActions(viewer).Any(x => x.Item1 is HeroPowerAction))
                {
                    bool targeted = viewer.HeroPower.ValidTargetSelector != null;
                    if (gm.Player.HeroPower.CanClick() == targeted || gm.Player.HeroPower.CanStartAiming() != targeted)
                        throw new Exception("Hero power click/aim routing does not match server targets.");
                    var context = new ActionContext
                    { Source = gm.Player.Data, SourcePlayer = gm.Player.Data, Target = targeted ? gm.Player.Data : null };
                    if (!gm.TryFindNetworkAction(new HeroPowerAction(), context, out _))
                        throw new Exception("Hero power input cannot match the server option.");
                }
                var action = new SmokeAgent().GetNextAction(_networkState);
                _networkEngine.Resolve(_networkState, action.Item2, action.Item1);
                PublishNetwork();
                if (gm.ActivePlayerTurn) throw new Exception("Network input became available during playback.");
            }
            catch (Exception ex) { Debug.LogException(ex); _failed = true; }
        }
        if (!completed && !_failed && EditorApplication.timeSinceStartup < _deadline) return;
        Application.logMessageReceived -= OnLog;
        if (completed && !_failed)
        {
            var actions = state.History.Select(x => x.Action.GetType().Name).Distinct().ToList();
            bool covered = new[] { nameof(HeroPowerAction), nameof(SubmitMulliganAction), nameof(SummonMinionAction), nameof(AttackAction), nameof(DamageAction), nameof(DeathAction) }.All(actions.Contains);
            if (covered) Debug.Log((_networkState == null ? "LOCAL" : $"NETWORK SEAT {_viewerSeat + 1}") + " PRESENTATION SMOKE PASSED: " + string.Join(", ", actions));
            else Debug.LogError("LOCAL PRESENTATION SMOKE FAILED: missing coverage: " + string.Join(", ", actions));
        }
        else Debug.LogError("LOCAL PRESENTATION SMOKE FAILED: playback error or timeout.");
        EditorApplication.isPlaying = false;
    }

    private static void ValidateDisplayedPlayer(Player view, CardBattleEngine.Player state, bool hidden)
    {
        if (view.Health != state.Health || view.Mana != state.Mana || view.Armor != state.Armor ||
            view.Hand.Cards.Count != state.Hand.Count || view.Board.Minions.Count != state.Board.Count)
            throw new Exception("Displayed player differs from the authoritative snapshot.");
        if (view.HeroPower.Data?.Name != state.HeroPower?.Name ||
            view.HeroPower.Data?.UsedThisTurn != state.HeroPower?.UsedThisTurn ||
            view.HeroPower.OriginalCard == null || !view.HeroPower.gameObject.activeSelf)
            throw new Exception("Displayed leader power differs from the authoritative snapshot.");
        for (int i = 0; i < view.Board.Minions.Count; i++)
            if (view.Board.Minions[i].Data.Id != state.Board[i].Id || view.Board.Minions[i].Health != state.Board[i].Health)
                throw new Exception("Displayed minion differs from the authoritative snapshot.");
        if (hidden && view.Hand.Cards.Any(x => x.Data != null)) throw new Exception("Opponent hand identity was exposed.");
    }

    private sealed class SmokeAgent : IGameAgent
    {
        public (IGameAction, ActionContext) GetNextAction(GameState game)
        {
            var player = game.PendingChoice?.SourcePlayer ?? game.CurrentPlayer;
            var options = game.PendingChoice != null ? game.PendingChoice.GetActions(game).ToList() : game.GetValidActions(player);
            return options.OrderBy(x => x.Item1 is PlayCardAction ? 0 : x.Item1 is AttackAction ? 1 : x.Item1 is EndTurnAction ? 3 : 2).First();
        }
        public void SetPlayer(CardBattleEngine.Player player) { }
        public void OnGameEnd(GameState state, bool win) { }
    }
}
