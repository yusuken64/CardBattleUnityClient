using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Dungeon : MonoBehaviour
{
	public TextMeshProUGUI TitleText;
	public TextMeshProUGUI ProgressText;
	public TextMeshProUGUI ResultText;

	public StoryModeScene StoryModeScene;
	public Map Map;

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
		DungeonSaveData currentDungeonSaveData = gameSaveData.StorySaveData.CurrentDungeon;
		var currentDungeon = Map.MapLocations
			.SelectMany(x => x.MapRegionDefinition.Dungeons)
			.FirstOrDefault(x => x.DungeonID == currentDungeonSaveData.ID);

		var encounter = currentDungeon.StoryModeDungeonEncounterDefinitions[UnityEngine.Random.Range(0, currentDungeon.StoryModeDungeonEncounterDefinitions.Count())];
		//TODO handle boss, if defined and wins == maxwins - 1;

		DeckSaveData firstDeck = gameSaveData.DeckSaveDatas[0];
		GameStartParams gameStartParams = new()
		{
			CombatDeck = firstDeck.ToDeck(),
			Health = 30,
			CombatDeckEnemy = encounter.Deck.ToDeck(),
			OpponentHealth = encounter.Health,
		};

		GameManager.GameStartParams = gameStartParams;
		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			var save = Common.Instance.SaveManager.SaveData.GameSaveData;

			if (isWin)
			{
				save.StorySaveData.CurrentDungeon.Wins++;
				save.StorySaveData.SetToComplete(save.StorySaveData.CurrentDungeon.ID);
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
			StoryModeScene.ReloadDungeonState();
		});
	}

	[ContextMenu("Finish Dungeon")]
	public void Debug_FinishDungeon()
	{
		var save = Common.Instance.SaveManager.SaveData.GameSaveData;
		save.StorySaveData.CurrentDungeon.Wins = save.StorySaveData.CurrentDungeon.MaxWins;
		save.StorySaveData.SetToComplete(save.StorySaveData.CurrentDungeon.ID);
		Setup();
	}
}
