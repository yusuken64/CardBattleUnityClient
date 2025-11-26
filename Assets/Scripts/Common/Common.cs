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

	public DeckDefinition StartingDeck;
	public Deck CurrentDeck { get; internal set; }

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
		SaveManager = new();
		GameSaveData = SaveManager.Load();

		if (GameSaveData.Decks.Count() == 0)
		{
			GameSaveData.Decks.Add(StartingDeck.ToDeck());
		}
		CurrentDeck = GameSaveData.Decks[0];
	}

	//public GameSaveData GameSaveData;

	//public AudioManager AudioManager;
	//public ItemManager ItemManager;
	//public SkillManager SkillManager;
	public GameObject SceneTransferObjects;


	//public ScreenTransition ScreenTransition;
	//public MessageDialog MessageDialog;

	//public GlobalSettings GlobalSettings;

	//	protected override void Initialize()
	//	{
	//		LoadData();
	//#if !UNITY_EDITOR
	//		SceneManager.LoadScene(1);
	//#endif
	//	}

	private void LoadData()
	{
		//GameSaveData = SaveSystem.LoadData();
	}
}

public class LoadingSceneIntegration
{
#if UNITY_EDITOR
	public static int otherScene = -2;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	static void InitLoadingScene()
	{
		int sceneIndex = SceneManager.GetActiveScene().buildIndex;
		if (sceneIndex == 0)
		{
			otherScene = 1;
		};

		otherScene = sceneIndex;
		//make sure your _preload scene is the first in scene build list
		AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(0);
		asyncOperation.completed += AsyncOperation_completed;
	}

	private static void AsyncOperation_completed(AsyncOperation obj)
	{
		SceneManager.LoadScene(otherScene);
	}
#endif
}