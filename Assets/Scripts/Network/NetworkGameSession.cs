using System;
using System.Threading;
using System.Threading.Tasks;

// Owned and pumped by NetworkManager. Buffers events until the game scene subscribes.
public sealed class NetworkGameSession
{
	public MiniSignalRClient Client { get; }
	public PlayerGameView LatestView { get; private set; }
	public bool HasEnded { get; private set; }
	public bool Disconnected { get; private set; }
	public string MatchId { get; set; }
	private readonly System.Collections.Generic.Queue<Action> _buffer = new();
	private Action<PlayerGameView> _stateUpdated;
	private Action<string> _actionRejected;
	private Action<Guid?> _matchEnded;
	private Action _disconnected;
	private readonly CancellationTokenSource _lifetime = new();
	private Task _stopTask;
	private bool _stopped;
	public CancellationToken CancellationToken => _lifetime.Token;

	public Task ConnectAsync() => Client.ConnectAsync(_lifetime.Token);

	public Task StopAsync()
	{
		if (_stopTask != null) return _stopTask;
		_stopped = true;
		Detach();
		_buffer.Clear();
		_lifetime.Cancel();
		return _stopTask = DisconnectAsync();
	}

	private async Task DisconnectAsync()
	{
		try { await Client.DisconnectAsync(); }
		catch (Exception ex) { UnityEngine.Debug.LogWarning($"Network cleanup failed: {ex.Message}"); }
	}

	public NetworkGameSession(string serverUrl)
	{
		Client = new MiniSignalRClient($"{serverUrl.TrimEnd('/')}/hubs/match");
		Client.On<PlayerGameView>("OnStateUpdated", view =>
		{
			LatestView = view;
			Dispatch(() => _stateUpdated(view));
		});
		Client.On<string>("OnActionRejected", error => Dispatch(() => _actionRejected(error)));
		Client.On<Guid?>("OnMatchEnded", winner =>
		{
			HasEnded = true;
			Dispatch(() => _matchEnded(winner));
		});
		Client.On<string>("OnMatchFound", id => MatchId = id);
		Client.OnDisconnected += () =>
		{
			Disconnected = true;
			Dispatch(() => _disconnected());
		};
		CardArtNetworkService.Initialize(Client);
	}

	public bool IsReady => !_stopped && Client.IsConnected && !Disconnected && !HasEnded &&
		!string.IsNullOrEmpty(MatchId) && LatestView != null && !LatestView.IsGameOver &&
		LatestView.ViewerPlayerId != Guid.Empty && LatestView.Self != null &&
		LatestView.Self.PlayerId == LatestView.ViewerPlayerId &&
		LatestView.Opponent != null && LatestView.Opponent.PlayerId != Guid.Empty &&
		LatestView.Opponent.PlayerId != LatestView.ViewerPlayerId;

	private void Dispatch(Action action)
	{
		if (_stopped) return;
		if (_stateUpdated == null) _buffer.Enqueue(action);
		else action();
	}

	public void Attach(Action<PlayerGameView> stateUpdated, Action<string> actionRejected,
		Action<Guid?> matchEnded, Action disconnected)
	{
		if (_stopped) throw new InvalidOperationException("The network session has stopped.");
		_stateUpdated = stateUpdated;
		_actionRejected = actionRejected;
		_matchEnded = matchEnded;
		_disconnected = disconnected;
		while (_buffer.Count > 0) _buffer.Dequeue()();
	}

	public void Detach()
	{
		_stateUpdated = null;
		_actionRejected = null;
		_matchEnded = null;
		_disconnected = null;
	}
}
