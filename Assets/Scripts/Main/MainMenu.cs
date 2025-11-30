using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject startobect;
	public GameObject SettingsObject;

	private void Start()
	{
		startobect.gameObject.SetActive(true);
		SettingsObject.gameObject.SetActive(false);
	}

	public void Play_Click()
	{
		SceneManager.LoadScene("GameScene");
	}

	public void Deck_Click()
	{
		SceneManager.LoadScene("DeckBuilder");
	}

	public void Settings_Click()
	{
		SettingsObject.gameObject.SetActive(true);
	}

	public void GlobalSettings_Click()
	{
		Common.Instance.GlobalSettings.gameObject.SetActive(true);
	}

	public void Close_Click()
	{
		Application.Quit();
	}
}
