using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject startobect;

	private void Start()
	{
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
