using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public GameObject startobect;
	public GameObject SettingsObject;
	public GameObject DataObject;

	private void Start()
	{
		startobect.gameObject.SetActive(true);
		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);
	}

	public void Play_Click()
	{
		DeckSaveData firstDeck = Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas[0];
		Common.Instance.SaveManager.SaveData.GameSaveData.CombatDeck = firstDeck;
		SceneManager.LoadScene("StoryMode");
	}

	public void Adventure_Click()
	{
		SceneManager.LoadScene("Adventure");
	}

	public void Deck_Click()
	{
		SceneManager.LoadScene("DeckBuilder");
	}

	public void OpenPacks_Click()
	{
		SceneManager.LoadScene("OpenPacks");
	}

	public void Settings_Click()
	{
		SettingsObject.gameObject.SetActive(true);
		DataObject.gameObject.SetActive(false);
	}

	public void Data_Click()
	{
		DataObject.gameObject.SetActive(true);
	}

	public void Reset_Click()
	{
		Common.Instance.SaveManager.ResetData();
		Common.Instance.SaveManager.EnsureData();
		Common.Instance.SaveManager.Save();
		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);
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
