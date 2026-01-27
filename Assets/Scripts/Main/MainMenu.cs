using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
	public GameObject SettingsObject;
	public GameObject DataObject;

	public DeckDefinition TutorialPlayerDeck;
	public DeckDefinition TutorialOpponentDeck;

	public Image Gradient;

	private void Start()
	{
		SettingsObject.gameObject.SetActive(false);
		DataObject.gameObject.SetActive(false);
		
		Flicker();
	}

	void Flicker()
	{
		Gradient
			.DOFade(UnityEngine.Random.Range(0f, 0.02f), 0.06f)
			.SetEase(Ease.InOutSine)
			.OnComplete(Flicker);
	}

	public void Play_Click()
	{
		if (ShouldRunTutorial())
		{
			Common.Instance.SceneTransition.DoTransition(LoadGameWithTutorial);
		}
		else
		{
			Common.Instance.SceneTransition.DoTransition(() =>
			{
				SceneManager.LoadScene("StoryMode");
			});
		}
	}

	private bool ShouldRunTutorial()
	{
		return !Common.Instance.SaveManager.SaveData.TutorialSaveData.BattleTutorialCompleted;
	}

	private IEnumerator LoadGameWithTutorial()
	{
		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			if (isWin)
			{
				Common.Instance.SaveManager.SaveData.TutorialSaveData.BattleTutorialCompleted = true;
			}

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
