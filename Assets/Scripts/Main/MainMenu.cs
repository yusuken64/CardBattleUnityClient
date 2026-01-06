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

		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("StoryMode");
		});
	}

	public void Adventure_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("Adventure");
		});
	}

	public void Deck_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("DeckBuilder");
		});
	}

	public void OpenPacks_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("OpenPacks");
		});
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

		SceneManager.LoadScene("Main");
	}

	public void AudioSettings_Click()
	{
		Common.Instance.GlobalSettings.gameObject.SetActive(true);
		Common.Instance.GlobalSettings.SetToAudioSettings();
	}

	public void DisplaySettings_Click()
	{
		Common.Instance.GlobalSettings.gameObject.SetActive(true);
		Common.Instance.GlobalSettings.SetToDisplaySettings();
	}

	public void Close_Click()
	{
		Common.Instance.SaveManager.Save();
		Application.Quit();
	}
}
