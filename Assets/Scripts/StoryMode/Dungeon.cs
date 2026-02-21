using DG.Tweening;
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

	public bool GameOver;

	public Image ScrollingBackground;
	public GameObject StartPositionObect;
	public GameObject EndPositionObect;
	private static int? previousWins;

	public void Setup()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;

		var currentDungeonSaveData = gameSaveData?.StorySaveData?.CurrentDungeon;
		if (currentDungeonSaveData == null)
		{
			//assume debug mode
			return;
		}

		DoBackgroundScroll(currentDungeonSaveData);

		TitleText.text = currentDungeonSaveData.Title;
		ProgressText.text = $"({currentDungeonSaveData.Wins}/{currentDungeonSaveData.MaxWins})";
		if (currentDungeonSaveData.Lives <= 0)
		{
			//game over
			ResultText.text = "Failed";
			BattleButton.interactable = false;
			GameOver = true;
		}
		else if (currentDungeonSaveData.Wins >= currentDungeonSaveData.MaxWins)
		{
			//complete
			ResultText.text = @"Complete
Acquired 1 Pack";
			BattleButton.interactable = false;
			GameOver = true;
		}
		else
		{
			ResultText.text = "";
			BattleButton.interactable = true;
			GameOver = false;
		}
	}

	private void DoBackgroundScroll(DungeonSaveData currentDungeonSaveData)
	{
		var backgroundTransform = ScrollingBackground.transform;

		var region = Map.MapLocations
			.FirstOrDefault(loc =>
				loc.MapRegionDefinition.Dungeons
					.Any(d => d.DungeonID == currentDungeonSaveData.ID));

		if (region == null)
		{
			Debug.LogError("Region not found for dungeon ID: " + currentDungeonSaveData.ID);
			return;
		}

		ScrollingBackground.sprite = region.MapRegionDefinition.DungeonSprite;

		int maxWins = currentDungeonSaveData.MaxWins;
		int wins = currentDungeonSaveData.Wins;

		float denominator = Mathf.Max(1, maxWins - 1);

		float t = Mathf.Clamp01(wins / denominator);

		float previousT = previousWins.HasValue
			? Mathf.Clamp01(previousWins.Value / denominator)
			: t;

		float startY = Mathf.Lerp(
			StartPositionObect.transform.position.y,
			EndPositionObect.transform.position.y,
			previousT);

		float endY = Mathf.Lerp(
			StartPositionObect.transform.position.y,
			EndPositionObect.transform.position.y,
			t);

		Vector3 startPosition = new Vector3(backgroundTransform.position.x, startY, backgroundTransform.position.z);
		Vector3 endPosition = new Vector3(backgroundTransform.position.x, endY, backgroundTransform.position.z);

		backgroundTransform.position = startPosition;

		backgroundTransform.DOMove(endPosition, 5f)
			.SetEase(Ease.Linear);
	}

	public void Fight_Clicked()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
		DungeonSaveData currentDungeonSaveData = gameSaveData.StorySaveData.CurrentDungeon;
		var currentDungeon = Map.MapLocations
			.SelectMany(x => x.MapRegionDefinition.Dungeons)
			.FirstOrDefault(x => x.DungeonID == currentDungeonSaveData.ID);

		previousWins = currentDungeonSaveData.Wins;
		var encounter = currentDungeon.StoryModeDungeonEncounterDefinitions[UnityEngine.Random.Range(0, currentDungeon.StoryModeDungeonEncounterDefinitions.Count())];
		//TODO handle boss, if defined and wins == maxwins - 1;

		DeckSaveData firstDeck = gameSaveData.DeckSaveDatas[0];
		GameStartParams gameStartParams = new()
		{
			CombatDeck = firstDeck.ToDeck(),
			Health = 30,
			CombatDeckEnemy = encounter.Deck.ToDeck(),
			OpponentHealth = encounter.Health,
			BackgroundName = encounter.BackgroundName
		};

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
					save.StorySaveData.SetToComplete(save.StorySaveData.CurrentDungeon.ID);
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
		if (GameOver)
		{
			ExitDungeon();
		}
		else
		{
			Common.Instance.YesNoConfirmation.Setup(
				"Exit Dungeon?",
				"You haven't finished this dungeon yet. Leaving now will reset your progress.",
				"Continue Run",
				() => { },
				"Leave Dungeon",
				() =>
				{
					ExitDungeon();
				});
		}
	}

	private void ExitDungeon()
	{
		previousWins = null;
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

	[ContextMenu("Reset Dungeon")]
	public void Debug_ResetDungeon()
	{
		var save = Common.Instance.SaveManager.SaveData.GameSaveData;
		save.StorySaveData.CurrentDungeon.Wins = 0;
		Setup();
	}

	[ContextMenu("Advance Dungeon")]
	public void Debug_AdvanceDungeon()
	{
		var save = Common.Instance.SaveManager.SaveData.GameSaveData;
		previousWins = save.StorySaveData.CurrentDungeon.Wins;
		save.StorySaveData.CurrentDungeon.Wins++;
		Setup();
	}
}
