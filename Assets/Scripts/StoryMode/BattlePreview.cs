using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattlePreview : MonoBehaviour
{
	public Image BattleImage;
	public TextMeshProUGUI DescriptionText;
	private StoryModeBattleDefinition _data;

	public Rondel Rondel;

	internal void Setup(StoryModeBattleDefinition data)
	{
		this._data = data;

		BattleImage.sprite = data.BattleImage;
		DescriptionText.text = data.Description;

		Rondel.Setup(data);
	}

	public void Fight_Clicked()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;

		var activeEffects = Rondel.GetActiveEffects();

		DeckSaveData firstDeck = gameSaveData.DeckSaveDatas[0];
		GameStartParams gameStartParams = new();
		gameStartParams.CombatDeck = firstDeck.ToDeck();
		gameStartParams.CombatDeckEnemy = _data.Deck.ToDeck();
		gameStartParams.OpponentExtraEffects = activeEffects.SelectMany(x => x.TriggeredEffects).ToList();
		GameManager.GameStartParams = gameStartParams;

		var levelId = _data.LevelID;

		GameManager.GameResultRoutine = GameResult;

		IEnumerator GameResult(bool isWin)
		{
			if (!isWin)
			{
				yield break;
			}

			var save = Common.Instance.SaveManager.SaveData.GameSaveData;
			var completed = save.StorySaveData.CompletedLevels;

			if (!completed.Contains(levelId))
			{
				completed.Add(levelId);
			}

			save.PackCount++;

			FindFirstObjectByType<UI>().ShowMessage("Acquired 1 Pack");

			Common.Instance.SaveManager.Save();

			yield return new WaitUntil(() => AnyInput());
		}

		GameManager.ReturnScreenName = "StoryMode";

		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("GameScene");
		});
	}

	private bool AnyInput()
	{
		return
			Mouse.current?.leftButton.wasPressedThisFrame == true ||
			Keyboard.current?.anyKey.wasPressedThisFrame == true ||
			Touchscreen.current?.primaryTouch.press.wasPressedThisFrame == true;
	}
}
