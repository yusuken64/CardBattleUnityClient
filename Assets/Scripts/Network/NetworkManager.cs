using System.Threading.Tasks;
using UnityEngine;

public sealed class NetworkManager : MonoBehaviour
{
	private bool _initialized;
	private bool _destroyed;
	public NetworkGameSession Session { get; private set; }
	public string ServerUrl { get; private set; } = NetworkClientConfig.DefaultServerUrl;
#if UNITY_EDITOR
	[Header("Multiplayer testing (Editor only)")]
	[Tooltip("Override client-config.json when entering Play mode. Never used in player builds.")]
	[SerializeField] private bool useEditorServerOverride;
	[Tooltip("HTTP(S) base URL without /hubs/match. Restart Play mode after changing this.")]
	[SerializeField] private string editorServerUrl = "http://localhost:5299";
#endif
	private double _nextNetworkRequestTime;
	public bool CanStartSession => _initialized && !_destroyed && Session == null &&
		Time.realtimeSinceStartupAsDouble >= _nextNetworkRequestTime;

	public bool TryCreateSession(out NetworkGameSession session)
	{
		session = null;
		if (!CanStartSession) return false;
		// Shared across dialogs and scenes; cancellation and failures do not reset it.
		_nextNetworkRequestTime = Time.realtimeSinceStartupAsDouble + 5;
		session = Session = new NetworkGameSession(ServerUrl);
		return true;
	}

	public async Task EndSessionAsync(NetworkGameSession session)
	{
		if (session == null) return;
		// Cleanup from an old scene must never clear a newer session.
		if (Session == session) Session = null;
		await session.StopAsync();
	}

	private void Update()
	{
		if (_initialized && !_destroyed) Session?.Client.PumpMainThread();
	}

	private void OnDestroy()
	{
		_destroyed = true;
		_ = EndSessionAsync(Session);
	}

	// Called explicitly by the persistent Common instance, independent of Awake order.
	public void Initialize()
	{
		if (_initialized || _destroyed) return;
		ServerUrl = NetworkClientConfig.ResolveServerUrl(NetworkClientConfig.DefaultServerUrl);
#if UNITY_EDITOR
		if (useEditorServerOverride)
		{
			if (NetworkClientConfig.TryNormalizeServerUrl(editorServerUrl, out var overrideUrl))
				ServerUrl = overrideUrl;
			else
				Debug.LogWarning("Invalid Editor server override on NetworkManager. Using the configured server.", this);
		}
#endif
		_initialized = true;
	}
}
