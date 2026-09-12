using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Common : MonoBehaviour
{
	public static Common Instance;

	public QuestTracker QuestTracker;
	public CardManager CardManager;
	public SaveManager SaveManager;
	public AudioManager AudioManager;
	public GlobalSettings GlobalSettings;
	public VideoSettingsManager VideoSettingsManager;
	public SceneTransition SceneTransition;
	public YesNoConfirmation YesNoConfirmation;
	public ModManager ModManager;

	public DeckDefinition StartingDeck;
	public NetworkGameSession NetworkSession { get; private set; }
	private double _nextNetworkRequestTime;
	public bool CanStartNetworkSession => NetworkSession == null &&
		Time.realtimeSinceStartupAsDouble >= _nextNetworkRequestTime;

	public bool TryCreateNetworkSession(string serverUrl, out NetworkGameSession session)
	{
		session = null;
		if (!CanStartNetworkSession) return false;
		// Shared across dialogs and scenes; cancellation and failures do not reset it.
		_nextNetworkRequestTime = Time.realtimeSinceStartupAsDouble + 5;
		session = NetworkSession = new NetworkGameSession(serverUrl);
		return true;
	}

	public async Task EndNetworkSessionAsync(NetworkGameSession session)
	{
		if (session == null) return;
		// Cleanup from an old scene must never clear a newer session.
		if (NetworkSession == session) NetworkSession = null;
		await session.StopAsync();
	}

	private void Update()
	{
		if (Instance == this) NetworkSession?.Client.PumpMainThread();
	}

	private void OnDestroy()
	{
		if (Instance != this) return;
		_ = EndNetworkSessionAsync(NetworkSession);
		Instance = null;
	}

	private void Awake()
	{
		Debug.Log("BOOT: Awake");
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this.transform);
		}
		else
		{
			Debug.Log("Duplicate Instance", this);
			//throw new System.Exception("Duplicate instance");
		}
	}

	private void Start()
	{
		Debug.Log("BOOT: Start");
		SaveManager.Initialize();
		SaveManager.Load();
		SaveManager.EnsureData();

		CardManager.ReloadCards();

		Debug.Log("AudioManager Initializing");
		AudioManager.ApplicationInitialized(SaveManager.SaveData);
		Debug.Log("AudioManager Initialized");

		YesNoConfirmation.gameObject.SetActive(false);
		VideoSettingsManager.InitializeVideo();
	}
}

public class LoadingSceneIntegration
{
	public static int otherScene = -2;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void InitLoadingScene()
	{
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		Debug.Log($"original sceneIndex, {sceneIndex}");
		if (sceneIndex == 0)
		{
			sceneIndex = 1;
		};

		otherScene = sceneIndex;
		//make sure your _preload scene is the first in scene build list
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(0);
		asyncOperation.completed += AsyncOperation_completed;
	}

	private static void AsyncOperation_completed(AsyncOperation obj)
	{
		Debug.Log($"post load sceneIndex, {otherScene}");
		SceneManager.LoadScene(otherScene);
	}
}
