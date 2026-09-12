using System;
using System.Linq;
using UnityEngine;

public partial class GameManager
{
    private PlayerGameView _displayedNetworkView;
    private bool _networkEnded;
    private Guid? _networkWinner;
    private bool _resultPresented;
    private bool _localResolutionPending;
    private bool _localAgentPending;
    private bool CanSubmitNetworkAction => _networkClient != null && !_networkSubmissionPending && !_networkUnavailable &&
        !_networkEnded && _lastNetworkView?.IsGameOver == false && _lastNetworkView.PromptVersion != null &&
        NetworkAnimationQueue != null && !NetworkAnimationQueue.IsProcessing &&
        NetworkAnimationQueue.DisplayedRevision == _lastNetworkView.StateRevision;

    internal void OnNetworkViewDisplayed(PlayerGameView view) => _displayedNetworkView = view;

    internal void ApplyPresentationStatus(ActionPresentation presentation)
    {
        foreach (var change in presentation.ResolvedStatusChanges)
        {
            var minion = GetObjectFor(change.Target)?.GetComponent<Minion>();
            if (minion == null) continue;
            if (change.Status == CardBattleEngine.StatusType.Freeze) minion.IsFrozen = change.Gained;
            if (change.Status == CardBattleEngine.StatusType.Stealth) minion.HasStealth = change.Gained;
            minion.UpdateUI();
        }
    }

    internal void OnPresentationQueueDrained()
    {
        if (Args?.Mode != GameMode.Networked) { CompleteLocalPresentation(); return; }
        if (NetworkAnimationQueue == null || NetworkAnimationQueue.IsProcessing) return;
        var view = _displayedNetworkView;
        if (view == null || view.StateRevision != _lastNetworkView?.StateRevision) return;
        ActivePlayerTurn = CanSubmitNetworkAction && (view.LegalActions?.Count ?? 0) > 0;
        OpponentTurn = view.CurrentPlayerId != LocalPlayerId;
        UpdateNetworkInteractionState(view);
        ShowDisplayedNetworkPrompt(view);
        if (!_resultPresented && (view.IsGameOver || _networkEnded))
        {
            _resultPresented = true;
            ActivePlayerTurn = false;
            UpdateNetworkInteractionState(view);
            bool won = (view.WinnerPlayerId ?? _networkWinner) == LocalPlayerId;
            var routine = FindFirstObjectByType<UI>()?.DoGameEndRoutine(won);
            if (routine != null) StartCoroutine(routine);
        }
    }

    private void CompleteLocalPresentation()
    {
        if (!_localResolutionPending || _gameState == null || AnimationQueue.IsPlaying) return;
        _localResolutionPending = false;
        if (_gameState.IsGameOver())
        {
            ActivePlayerTurn = false;
            if (!_resultPresented)
            {
                _resultPresented = true;
                StartCoroutine(FindFirstObjectByType<UI>().DoGameEndRoutine(_gameState.Winner?.Id == LocalPlayerId));
            }
            return;
        }
        var next = _gameState.PendingChoice?.SourcePlayer ?? _gameState.CurrentPlayer;
        ActivePlayerTurn = next.Id == LocalPlayerId;
        OpponentTurn = !ActivePlayerTurn;
        Player.UpdatePlayableActions(ActivePlayerTurn);
        Opponent.UpdatePlayableActions(false);
        var ui = FindFirstObjectByType<UI>();
        if (ActivePlayerTurn) ui.EndTurnButton.SetToReady();
        else ui.EndTurnButton.SetToEnemyTurn();
        if (ActivePlayerTurn && _gameState.PendingChoice?.GetType().Name == "MulliganChoce")
        {
            var prompt = MulliganPrompt ?? FindFirstObjectByType<MulliganPrompt>(FindObjectsInactive.Include);
            prompt.gameObject.SetActive(true);
            prompt.Setup(Player.Hand.Cards);
        }
        var agent = ActivePlayerTurn ? _playerAgent : _opponentAgent;
        if (agent != null && !_localAgentPending) RequestLocalMove(agent);
    }

    private async void RequestLocalMove(IGameAgent agent)
    {
        _localAgentPending = true;
        try { await ProcessMoveAsync(agent); }
        finally
        {
            _localAgentPending = false;
            if (this != null && !AnimationQueue.IsPlaying) CompleteLocalPresentation();
        }
    }

    private void ShowDisplayedNetworkPrompt(PlayerGameView view)
    {
        if (_networkEnded || view.IsGameOver) { MulliganPrompt?.gameObject.SetActive(false); return; }
        if (!_mulliganHandled && view.Self?.Hand != null && view.Self.Hand.Count > 0)
        {
            var hand = view.Self.Hand.Select(cv => CardBuilder.BuildCard(cv, null)).ToList();
            MulliganPrompt?.SetupNetworked(hand);
            _mulliganHandled = true;
        }
        if (view.PendingChoice != null) MulliganPrompt?.TryHandlePendingChoice(view.PendingChoice);
        if (_mulliganHandled && (view.LegalActions?.Count ?? 0) > 0 && view.PendingChoice == null &&
            view.CurrentPlayerId == LocalPlayerId && MulliganPrompt != null && MulliganPrompt.gameObject.activeSelf)
            MulliganPrompt.gameObject.SetActive(false);
    }

    // Keep surviving objects and their transforms. Only actual zone changes create/remove views.
    public void ReconcileBoardState(BoardStateSnapshot snapshot)
    {
        ReconcilePlayer(Player, snapshot.Self);
        ReconcilePlayer(Opponent, snapshot.Opponent);
        LocalPlayerId = snapshot.LocalPlayerId;
    }

    private void ReconcilePlayer(Player player, PlayerBoardSnapshot snapshot)
    {
        var handler = FindFirstObjectByType<GameInteractionHandler>();
        player.Data = snapshot.Data;
        var desiredCards = snapshot.Data.Hand;
        bool hidden = player == Opponent;
        foreach (var card in player.Hand.Cards.ToList())
        {
            if (card == null) { player.Hand.Cards.Remove(card); continue; }
            if (!hidden && !desiredCards.Any(x => x.Id == card.Data?.Id))
            { player.Hand.Cards.Remove(card); Destroy(card.gameObject); }
        }
        if (hidden)
        {
            while (player.Hand.Cards.Count > snapshot.HiddenHandCount)
            {
                var card = player.Hand.Cards[player.Hand.Cards.Count - 1];
                player.Hand.Cards.RemoveAt(player.Hand.Cards.Count - 1);
                Destroy(card.gameObject);
            }
            while (player.Hand.Cards.Count < snapshot.HiddenHandCount)
                player.Hand.AddCard(Instantiate(handler.CardPrefab, player.Hand.transform));
        }
        else
        {
            foreach (var cardData in desiredCards)
            {
                var card = player.Hand.Cards.FirstOrDefault(x => x.Data?.Id == cardData.Id);
                if (card == null) { card = Instantiate(handler.CardPrefab, player.Hand.transform); player.Hand.Cards.Add(card); }
                card.Setup(cardData);
            }
            player.Hand.Cards.Sort((a,b) => desiredCards.FindIndex(x => x.Id == a.Data.Id).CompareTo(desiredCards.FindIndex(x => x.Id == b.Data.Id)));
        }
        var desiredMinions = snapshot.Data.Board.OfType<CardBattleEngine.Minion>().ToList();
        foreach (var minion in player.Board.Minions.ToList())
        {
            if (minion == null) { player.Board.Minions.Remove(minion); continue; }
            if (!desiredMinions.Any(x => x.Id == minion.Data?.Id))
            { player.Board.Minions.Remove(minion); Destroy(minion.gameObject); }
        }
        foreach (var data in desiredMinions)
        {
            var minion = player.Board.Minions.FirstOrDefault(x => x.Data?.Id == data.Id);
            if (minion == null) { minion = Instantiate(handler.MinionPrefab, player.Board.transform); player.Board.Minions.Add(minion); }
            minion.Setup(data);
        }
        player.Board.Minions.Sort((a,b) => desiredMinions.FindIndex(x => x.Id == a.Data.Id).CompareTo(desiredMinions.FindIndex(x => x.Id == b.Data.Id)));
        if (player.HeroPower != null)
        {
            player.HeroPower.Data = snapshot.Data.HeroPower;
            player.HeroPower.OriginalCard = snapshot.Data.HeroPower?.LeaderCard;
        }
        player.RefreshData();
        for (int i = 0; i < player.Board.Minions.Count; i++)
        {
            var minion = player.Board.Minions[i];
            var authoritative = snapshot.SourceMinionViews[i];
            minion.CanAttack = authoritative.CanAttack;
            minion.HasDeathRattle = authoritative.HasDeathRattle;
            minion.HasTrigger = authoritative.HasTrigger;
            minion.UpdateUI();
        }
        player.CardsLeftInDeck = snapshot.DeckCount;
        player.UpdateUI();
        player.Hand.UpdateCardPositions();
        player.Board.UpdateMinionPositions();
    }
}
