using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Common : MonoBehaviour
{
	public static Common Instance;

	[SerializeReference]
	public GameSaveData GameSaveData;

	public CardManager CardManager;
	public SaveManager SaveManager;
	public AudioManager AudioManager;

	public DeckDefinition StartingDeck;
	public int CurrentDeckIndex { get; internal set; } = -1;

	private void Awake()
	{
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
		SaveManager.Initialize();
		if (SaveManager.LoadDataAtStart)
		{
			GameSaveData = SaveManager.Load();
		}
		else
		{
			GameSaveData = new();
		}

		if (GameSaveData.DeckSaveDatas.Count() == 0)
		{
			GameSaveData.DeckSaveDatas.Add(StartingDeck.ToDeckData());
		}
		CurrentDeckIndex = 0;
		SaveManager.Save(GameSaveData);
	}

	//public GlobalSettings GlobalSettings;

	//	protected override void Initialize()
	//	{
	//		LoadData();
	//#if !UNITY_EDITOR
	//		SceneManager.LoadScene(1);
	//#endif
	//	}
}

public class LoadingSceneIntegration
{
#if UNITY_EDITOR
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
#endif
}