using System;
using UnityEngine;

// Network ordering only; visuals use the same prefabs as local games.
public class NetworkAnimationQueue : MonoBehaviour
{
    public GameManager GameManager;
    public bool IsProcessing => _resyncing || (GameManager?.AnimationQueue?.IsPlaying ?? false);
    public long DisplayedRevision { get; private set; }
    private long _sequence;
    private long _revision;
    private bool _initialized;
    private bool _resyncing;
    private bool _subscribed;
    public void Enqueue(PlayerGameView view)
    {
        if (view == null || _resyncing) return;
        if (!_subscribed) { GameManager.AnimationQueue.PlaybackFailed += Recover; _subscribed = true; }
        if (!_initialized) { ResetTo(view); return; }
        if (view.StateRevision <= _revision) return;
        var sequence = _sequence;
        foreach (var entry in view.PlaybackEvents)
        {
            if (entry.Sequence <= sequence) continue;
            if (entry.Sequence != sequence + 1) { Recover(); return; }
            sequence = entry.Sequence;
        }
        if (sequence != view.PlaybackSequence) { Recover(); return; }
        _revision = view.StateRevision;
        try
        {
            foreach (var entry in view.PlaybackEvents)
            {
                if (entry.Sequence <= _sequence) continue;
                _sequence = entry.Sequence;
                GameManager.AnimationQueue.Enqueue(GameManager, new ActionPresentation(entry));
            }
        }
        catch (Exception ex) { Debug.LogError($"Could not construct playback batch: {ex}"); Recover(); return; }
        GameManager.AnimationQueue.EnqueueBoundary(GameManager, () =>
        {
            GameManager.ReconcileBoardState(BoardStateSnapshot.FromPlayerGameView(view));
            DisplayedRevision = view.StateRevision;
            GameManager.OnNetworkViewDisplayed(view);
        });
    }
    public void ResetTo(PlayerGameView view)
    {
        if (view == null) throw new InvalidOperationException("Server returned no snapshot.");
        GameManager.AnimationQueue.Cancel(GameManager);
        _sequence = view.PlaybackSequence;
        _revision = DisplayedRevision = view.StateRevision;
        _initialized = true;
        _resyncing = false;
        GameManager.ApplyBoardState(BoardStateSnapshot.FromPlayerGameView(view));
        GameManager.OnNetworkViewDisplayed(view);
        GameManager.OnPresentationQueueDrained();
    }
    private async void Recover()
    {
        if (_resyncing) return;
        _resyncing = true;
        GameManager.AnimationQueue.Cancel(GameManager);
        GameManager.ActivePlayerTurn = false;
        try { await GameManager.ResyncFromServer(); }
        catch (Exception ex) { Debug.LogError($"Playback resynchronization failed: {ex}"); }
    }
    private void OnDestroy()
    {
        if (_subscribed && GameManager?.AnimationQueue != null)
            GameManager.AnimationQueue.PlaybackFailed -= Recover;
    }
}
