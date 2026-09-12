using System;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;
using Newtonsoft.Json.Linq;
using DG.Tweening;

// Captured once at the action boundary. Only Bind reads live local state, to attach input
// components to real engine entities; animation values always come from the captured frame.
public sealed class ActionPresentation
{
    public PlaybackEventView Event { get; }
    public BoardStateSnapshot Snapshot { get; }
    public bool IsNetwork => _liveState == null;
    public CustomSFX Effect { get; }
    private readonly GameState _liveState;
    private readonly GameState _localSnapshot;
    private readonly Dictionary<Guid, IGameEntity> _entities = new();
    private readonly List<UnityEngine.GameObject> _transients = new();

    public T Own<T>(T visual) where T : UnityEngine.Object
    {
        var gameObject = visual as UnityEngine.GameObject ?? (visual as UnityEngine.Component)?.gameObject;
        if (gameObject != null) _transients.Add(gameObject);
        return visual;
    }
    internal bool HasLiveVisuals => _transients.Any(x => x != null) || (DOTween.TweensById(this)?.Count ?? 0) > 0;
    internal void CancelVisuals()
    {
        DOTween.Kill(this);
        foreach (var visual in _transients) if (visual != null) UnityEngine.Object.Destroy(visual);
        _transients.Clear();
    }

    public ActionPresentation(PlaybackEventView frame, GameState liveState = null, CustomSFX effect = null)
    {
        Event = frame;
        _liveState = liveState;
        _localSnapshot = liveState?.Clone();
        Effect = effect ?? PresentationEffectRegistry.Resolve(frame.PresentationEffectId);
        Snapshot = BoardStateSnapshot.FromPlayerGameView(frame.After);
        foreach (var player in new[] { Snapshot.Self.Data, Snapshot.Opponent.Data })
        {
            _entities[player.Id] = player;
            foreach (var entity in player.Hand.Cast<IGameEntity>().Concat(player.Board)) _entities[entity.Id] = entity;
        }
        foreach (var entity in new[] { frame.Source, frame.Target, frame.SourceCard, frame.CardGained, frame.SummonedMinion, frame.TriggerSource }
            .Concat(frame.AffectedEntities.Select(x => x.Target)).Concat(frame.StatusChanges.Select(x => x.Target)))
            AddEntity(entity);
    }

    public static ActionPresentation Capture(GameManager gm, GameState state, (IGameAction action, ActionContext context) current)
    {
        var viewer = state.Players.First(x => x.Id == gm.LocalPlayerId);
        var frame = CardBattleEngine.View.PlayerViewBuilder.BuildPlayback(state, viewer, current.action, current.context, 0, includePrivate: true);
        return new ActionPresentation(JObject.FromObject(frame).ToObject<PlaybackEventView>(), state, current.action.CustomSFX as CustomSFX);
    }

    private void AddEntity(PlaybackEntityView entity)
    {
        if (entity == null || _entities.ContainsKey(entity.Id)) return;
        var owner = _entities.TryGetValue(entity.OwnerId, out var p) ? p as CardBattleEngine.Player : null;
        if (entity.Minion != null) _entities[entity.Id] = MinionBuilder.BuildMinion(entity.Minion, owner);
        else if (entity.Card != null) _entities[entity.Id] = CardBuilder.BuildCard(entity.Card, owner);
    }

    private IGameEntity Resolve(PlaybackEntityView entity) => entity != null && _entities.TryGetValue(entity.Id, out var data) ? data : null;
    public CardBattleEngine.Player SourcePlayer => _entities.TryGetValue(Event.PlayerId, out var p) ? p as CardBattleEngine.Player : null;
    public IGameEntity Source => Resolve(Event.Source);
    public IGameEntity Target => Resolve(Event.Target);
    public IGameEntity TriggerSource => Resolve(Event.TriggerSource);
    public CardBattleEngine.Card SourceCard => Resolve(Event.SourceCard) as CardBattleEngine.Card;
    public CardBattleEngine.Card CardGained => Resolve(Event.CardGained) as CardBattleEngine.Card;
    public CardBattleEngine.Minion SummonedMinion => Resolve(Event.SummonedMinion) as CardBattleEngine.Minion;
    public int PlayIndex => Event.PlayIndex;
    public int DamageDealt => Event.DamageDealt;
    public int HealedAmount => Event.HealedAmount;
    public int ArmorGained => Event.ArmorGained;
    public int CardsLeftInDeck => Event.CardsLeftInDeck;
    public int Fatigue => Event.After.Self.PlayerId == Event.PlayerId ? Event.After.Self.Fatigue : Event.After.Opponent.Fatigue;
    public bool IsAttack => Event.IsAttack;
    public List<(IGameEntity, int)> AffectedEntities => Event.AffectedEntities.Select(x => (Resolve(x.Target), x.Amount)).Where(x => x.Item1 != null).ToList();
    public IEnumerable<(IGameEntity Target, StatusType Status, bool Gained)> ResolvedStatusChanges => Event.StatusChanges.Select(x => (Resolve(x.Target), x.Status, x.Gained));
    private T Bind<T>(T entity) where T : class, IGameEntity => entity == null ? null : (_liveState?.GetAllEntities().FirstOrDefault(x => x.Id == entity.Id) as T ?? entity);

    public void Sync(GameManager gm)
    {
        if (IsNetwork) gm.ReconcileBoardState(Snapshot);
        else foreach (var entity in _localSnapshot.GetAllEntities())
        {
            var view = gm.GetObjectByID(entity.Id);
            if (view is Minion minion) minion.BindData(Bind(entity) as CardBattleEngine.Minion);
            if (view is Card card) card.BindData(Bind(entity) as CardBattleEngine.Card);
            view?.SyncData(entity);
        }
    }
}
