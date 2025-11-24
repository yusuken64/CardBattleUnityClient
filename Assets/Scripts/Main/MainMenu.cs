using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject startobect;

	public DeckDefinition StartingDeck;

	private void Start()
	{
		if (Common.Instance.GameSaveData.Decks.Count() == 0)
		{
			Common.Instance.GameSaveData.Decks.Add(StartingDeck.ToDeck());
		}
		Common.Instance.CurrentDeck = Common.Instance.GameSaveData.Decks[0];
		startobect.gameObject.SetActive(true);
	}

	public void Play_Click()
	{
		SceneManager.LoadScene("GameScene");
	}

	public void Deck_Click()
	{
		SceneManager.LoadScene("DeckBuilder");
	}

	public void Close_Click()
	{
		Application.Quit();
	}
}
