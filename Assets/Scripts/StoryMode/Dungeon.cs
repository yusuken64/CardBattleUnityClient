using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Dungeon : MonoBehaviour
{
	public TextMeshProUGUI TitleText;
	public TextMeshProUGUI ProgressText;
	public TextMeshProUGUI ResultText;

	public StoryModeScene StoryModeScene; //TODO replace with biomedata

	public Button BattleButton;
	public Button FinishButton;

	public void Setup()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;

		var currentDungeon = gameSaveData?.StorySaveData?.CurrentDungeon;
		if (currentDungeon == null)
		{
			//assume debug mode
			return;
		}

		TitleText.text = currentDungeon.Title;
		ProgressText.text = $"({currentDungeon.Wins}/{currentDungeon.MaxWins})";
		if (currentDungeon.Lives <= 0)
		{
			//game over
			ResultText.text = "Failed";
			BattleButton.interactable = false;
		}
		else if (currentDungeon.Wins >= currentDungeon.MaxWins)
		{
			//complete
			ResultText.text = @"Complete
Acquired 1 Pack";
			BattleButton.interactable = false;
		}
		else
		{
			ResultText.text = "";
			BattleButton.interactable = true;
		}
	}

	public void Fight_Clicked()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
		//var storyData = StoryModeScene.Datas[UnityEngine.Random.Range(0, StoryModeScene.Datas.Count)];

		DeckSaveData firstDeck = gameSaveData.DeckSaveDatas[0];
		GameStartParams gameStartParams = new();
		gameStartParams.CombatDeck = firstDeck.ToDeck();
		//gameStartParams.CombatDeckEnemy = storyData.Deck.ToDeck();
		//gameStartParams.OpponentExtraEffects = activeEffects.SelectMany(x => x.TriggeredEffects).ToList();
		GameManager.GameStartParams = gameStartParams;

		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			var save = Common.Instance.SaveManager.SaveData.GameSaveData;

			if (isWin)
			{
				save.StorySaveData.CurrentDungeon.Wins++;
				if (save.StorySaveData.CurrentDungeon.Wins >=
					save.StorySaveData.CurrentDungeon.MaxWins)
				{
					save.PackCount++;
				}
			}
			else
			{
				save.StorySaveData.CurrentDungeon.Lives--;
			}


			Common.Instance.SaveManager.Save();
			yield return null;
		}

		GameManager.ReturnScreenName = "StoryMode";

		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("GameScene");
		});
	}

	public void Exit_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CurrentDungeon.Exited = true;
			Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CurrentDungeon = null;
			Common.Instance.SaveManager.Save();

			this.gameObject.SetActive(false);
			StoryModeScene.Map.gameObject.SetActive(true); //TODO select this dungeon on map
		});
	}
}
