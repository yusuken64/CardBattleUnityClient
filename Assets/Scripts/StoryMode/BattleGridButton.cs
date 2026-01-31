using System;
using UnityEngine;
using UnityEngine.UI;

public class BattleGridButton : MonoBehaviour
{
	public Image BattleImage;
	public GameObject CompletedIndicator;

	private StoryModeDungeonDefinition _data;

	public Action<StoryModeDungeonDefinition> ClickAction;

	public void OnClick()
	{
		ClickAction?.Invoke(_data);
	}

	internal void Setup(StoryModeDungeonDefinition data)
	{
		this._data = data;
		BattleImage.sprite = data.BattleImage;

		bool completed = Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CompletedLevels.Contains(data.DungeonID);
		CompletedIndicator.SetActive(completed);
	}
}
