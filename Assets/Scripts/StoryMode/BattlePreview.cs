using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattlePreview : MonoBehaviour
{
	public Image BattleImage;
	public TextMeshProUGUI DescriptionText;
	private StoryModeBattleDefinition _data;

	internal void Setup(StoryModeBattleDefinition data)
	{
		this._data = data;

		BattleImage.sprite = data.BattleImage;
		DescriptionText.text = data.Description;
	}

	public void Fight_Clicked()
	{
		GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;

		DeckSaveData firstDeck = gameSaveData.DeckSaveDatas[0];
		gameSaveData.CombatDeck = firstDeck;
		gameSaveData.CombatDeckEnemy = _data.Deck.ToDeckData();
		var levelId = _data.LevelID;

		GameManager.GameResultAction = (isWin) =>
		{
			if (!isWin) return;

			var save = Common.Instance.SaveManager.SaveData.GameSaveData;
			var completed = save.StorySaveData.CompletedLevels;

			if (!completed.Contains(levelId))
			{
				completed.Add(levelId);
			}
		};

		GameManager.ReturnScreenName = "StoryMode";

		SceneManager.LoadScene("GameScene");
	}
}
