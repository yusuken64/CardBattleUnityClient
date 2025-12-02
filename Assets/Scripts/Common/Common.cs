using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Common : MonoBehaviour
{
	public static Common Instance;

	public SaveData SaveData;

	public CardManager CardManager;
	public SaveManager SaveManager;
	public AudioManager AudioManager;
	public GlobalSettings GlobalSettings;

	public DeckDefinition StartingDeck;
	public int CurrentDeckIndex { get; internal set; } = -1;

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
		SaveData = SaveManager.Load();

		if (SaveData.GameSaveData.DeckSaveDatas.Count() == 0)
		{
			CurrentDeckIndex = 0;
			SaveManager.Save(SaveData);
			SaveData.GameSaveData.DeckSaveDatas.Add(StartingDeck.ToDeckData());
		}

		Debug.Log("AudioManager Initializing");
		AudioManager.ApplicationInitialized(SaveData);
		Debug.Log("AudioManager Initialized");
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