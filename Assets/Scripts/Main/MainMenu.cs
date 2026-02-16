using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public Button ContinueButton;
	public Button NewGameButton;

	public GameObject SettingsObject;
	public GameObject DataObject;

	public DeckDefinition TutorialPlayerDeck;
	public DeckDefinition TutorialOpponentDeck;

	private bool showContinue;

	private void Start()
	{
		showContinue = ShouldShowContinue();
		ContinueButton.gameObject.SetActive(showContinue);

		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);
	}

	private bool ShouldShowContinue()
	{
		return Common.Instance.SaveManager.SaveData.GameSaveData.TutorialSaveData.BattleTutorialCompleted;
	}

	public void Continue_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("StoryMode");
		});
	}

	public void NewGame_Click()
	{
		if (showContinue)
		{
			//prompt are you sure;
			Common.Instance.YesNoConfirmation.Setup("Start New Game?",
				"this will overwrite current progress",
				"New Game",
				() =>
				{
					Common.Instance.SceneTransition.DoTransition(() =>
					{
						StartNameGame();
					});
				},
				"Cancel",
				() => { });
		}
		else
		{
			Common.Instance.SceneTransition.DoTransition(() =>
			{
				StartNameGame();
			});
		}
	}

	private void StartNameGame()
	{
		Common.Instance.SaveManager.ResetData();
		Common.Instance.SaveManager.EnsureData();
		Common.Instance.SaveManager.Save();

		SceneManager.LoadScene("StoryIntro");
		IntroCutscene.ExitAction = () =>
		{
			Common.Instance.SceneTransition.DoTransition(LoadGameWithTutorial);
			
			return true;
		};
	}

	private IEnumerator LoadGameWithTutorial()
	{
		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			if (isWin)
			{
				Common.Instance.SaveManager.SaveData.GameSaveData.TutorialSaveData.BattleTutorialCompleted = true;
			}

			Common.Instance.SaveManager.SaveData.GameSaveData.PackCount++;

			yield return null;
		}

		GameStartParams gameStartParams = new()
		{
			BlockStart = true,
			SkipMulligan = true,
			SkipShuffle = true,
			InitialCards = 0
		};
		gameStartParams.CombatDeck = TutorialPlayerDeck.ToDeckData().ToDeck();
		gameStartParams.CombatDeckEnemy = TutorialOpponentDeck.ToDeckData().ToDeck(); ;
		GameManager.GameStartParams = gameStartParams;
		GameManager.ReturnScreenName = "StoryMode";

		yield return SceneManager.LoadSceneAsync(
			"GameScene",
			LoadSceneMode.Single
		);

		yield return SceneManager.LoadSceneAsync(
			"GameSceneTutorial",
			LoadSceneMode.Additive
		);
	}

	public void Adventure_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("Adventure");
		});
	}

	public void Arena_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("Arena");
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

	public void ResetTutorial_Click()
	{
		Common.Instance.SaveManager.ResetTutorialData();
		Common.Instance.SaveManager.EnsureData();
		Common.Instance.SaveManager.Save();
		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);

		SceneManager.LoadScene("Main");
	}

	public void SkipTutorial_Click()
	{
		Common.Instance.SaveManager.SkipTutorialData();
		Common.Instance.SaveManager.EnsureData();
		Common.Instance.SaveManager.Save();
		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);

		SceneManager.LoadScene("Main");
	}

	public void AllCards_Click()
	{
		Common.Instance.CardManager.GiveAllCards();
		SceneManager.LoadScene("Main");
	}

	public void PacksPlus10_Click()
	{
		Common.Instance.SaveManager.SaveData.GameSaveData.PackCount += 10;
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

	public void CutScene_Click()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("StoryIntro");
		});
	}
}
