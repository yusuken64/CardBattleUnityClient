using System;
using System.Collections.Generic;
using System.Linq;
using CardBattleEngine;
using UnityEngine;

public partial class GameManager
{
    private sealed class PlayerActionIntent
    {
        public IGameAction Action;
        public ActionContext Context;
        public int Turn;
    }

    private readonly Queue<PlayerActionIntent> _playerActions = new Queue<PlayerActionIntent>();
    private bool _endTurnQueued;
    private int? _endingTurn;

    // Input ownership is independent of whether presentation is ready to send an action.
    public bool CanQueuePlayerAction
    {
        get
        {
            if (_resultPresented || _isDestroying || _endTurnQueued) return false;
            if (_endingTurn.HasValue && _endingTurn == (Args?.Mode == GameMode.Networked ? _lastNetworkView?.Turn : _gameState?.turn)) return false;
            if (Args?.Mode == GameMode.Networked)
                return !_networkUnavailable && !_networkEnded && _networkClient != null &&
                    CanQueueNetworkTurn(_lastNetworkView, _displayedNetworkView, LocalPlayerId);
            return _gameState != null && !_gameState.IsGameOver() &&
                _gameState.CurrentPlayer.Id == LocalPlayerId && _gameState.PendingChoice == null && !_localAgentPending;
        }
    }

    public static bool CanQueueNetworkTurn(PlayerGameView latest, PlayerGameView displayed, Guid? localPlayerId)
    {
        return latest != null && displayed != null && localPlayerId.HasValue && !latest.IsGameOver &&
            latest.CurrentPlayerId == localPlayerId && displayed.CurrentPlayerId == localPlayerId &&
            latest.Turn == displayed.Turn && latest.PendingChoice == null && !latest.OpponentIsChoosing;
    }

    public bool PlayerActionMustWait => Args?.Mode == GameMode.Networked
        ? !CanSubmitNetworkAction : (AnimationQueue?.IsPlaying ?? false) || _localResolutionPending;

    public void QueuePlayerAction(IGameAction action, ActionContext context)
    {
        if (!CanQueuePlayerAction || action == null || context?.SourcePlayer == null) return;
        if (_playerActions.Count >= 16) return;
        _playerActions.Enqueue(new PlayerActionIntent {
            Action = action,
            Context = new ActionContext(context) { PlayIndex = context.PlayIndex,
                Targets = context.Targets?.ToList() },
            Turn = Args?.Mode == GameMode.Networked ? _lastNetworkView.Turn : _gameState.turn
        });
        if (action is EndTurnAction) _endTurnQueued = true;
        DrainPlayerActions();
    }

    internal void ClearQueuedPlayerActions()
    {
        _playerActions.Clear();
        _endTurnQueued = false;
        _endingTurn = null;
    }

    // Match semantic identity again against the newest prompt; never reuse its old option index.
    public static LegalActionView FindQueuedNetworkAction(IEnumerable<LegalActionView> options,
        string actionType, Guid? sourceId, Guid? targetId)
    {
        return options?.FirstOrDefault(option => option.ActionType == actionType &&
            option.SourceEntityId == sourceId && option.TargetEntityId == targetId);
    }

    private bool DrainPlayerActions()
    {
        if (_resultPresented || _isDestroying || _networkUnavailable || _networkEnded ||
            (Args?.Mode == GameMode.Networked ? _lastNetworkView?.IsGameOver == true : _gameState?.IsGameOver() == true))
        { ClearQueuedPlayerActions(); return false; }
        if (_playerActions.Count == 0) return false;
        bool network = Args?.Mode == GameMode.Networked;
        int currentTurn = network ? _lastNetworkView.Turn : _gameState.turn;
        Guid currentPlayer = network ? _lastNetworkView.CurrentPlayerId : _gameState.CurrentPlayer.Id;
        bool choicePending = network ? _lastNetworkView.PendingChoice != null || _lastNetworkView.OpponentIsChoosing
            : _gameState.PendingChoice != null;
        if (currentTurn != _playerActions.Peek().Turn || currentPlayer != LocalPlayerId || choicePending)
        { ClearQueuedPlayerActions(); return false; }
        if (PlayerActionMustWait) return false;
        bool endQueued = _endTurnQueued;
        _endTurnQueued = false;
        bool ourTurn = CanQueuePlayerAction;
        _endTurnQueued = endQueued;
        if (!ourTurn) { ClearQueuedPlayerActions(); return false; }
        while (_playerActions.Count > 0)
        {
            var intent = _playerActions.Dequeue();
            if (intent.Action is EndTurnAction) _endTurnQueued = false;
            int turn = Args?.Mode == GameMode.Networked ? _lastNetworkView.Turn : _gameState.turn;
            if (turn != intent.Turn) { ClearQueuedPlayerActions(); return false; }
            var context = intent.Context;
            if (Args?.Mode == GameMode.Networked)
            {
                Guid? source = context.Source?.Id ?? context.SourceCard?.Id;
                Guid? target = context.Target?.Id;
                if (!target.HasValue && intent.Action is PlayCardAction play && play.Card is WeaponCard)
                    target = play.Card.Owner?.Id;
                var match = FindQueuedNetworkAction(_lastNetworkView.LegalActions, intent.Action.GetType().Name, source, target);
                if (match != null)
                {
                    if (intent.Action is EndTurnAction) _endingTurn = intent.Turn;
                    SubmitLocalAction(match);
                    return true;
                }
            }
            else
            {
                context.SourcePlayer = _gameState.GetEntityById(context.SourcePlayer.Id) as CardBattleEngine.Player;
                if (context.Source != null) context.Source = _gameState.GetEntityById(context.Source.Id);
                if (context.SourceCard != null) context.SourceCard = _gameState.GetEntityById(context.SourceCard.Id) as CardBattleEngine.Card;
                if (context.Targets != null)
                    context.Targets = context.Targets.Select(t => _gameState.GetEntityById(t.Id)).ToList();
                if (intent.Action is PlayCardAction play) play.Card = context.SourceCard;
                if (context.SourcePlayer != null && context.Source != null &&
                    !(context.Targets?.Any(t => t == null) ?? false) &&
                    (!(intent.Action is PlayCardAction) || context.SourceCard != null) &&
                    intent.Action.IsValid(_gameState, context, out _))
                {
                    if (intent.Action is EndTurnAction) _endingTurn = intent.Turn;
                    ResolveAction(intent.Action, context);
                    return true;
                }
            }
            FindFirstObjectByType<UI>()?.ShowMessage("Queued action is no longer available.");
        }
        _endTurnQueued = false;
        return false;
    }
}
