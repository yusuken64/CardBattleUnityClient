using System;
using UnityEngine;
using UnityEngine.UI;

public class BattleGridButton : MonoBehaviour
{
	public Image BattleImage;
	public GameObject CompletedIndicator;

	private StoryModeBattleDefinition _data;

	public Action<StoryModeBattleDefinition> ClickAction;

	public void OnClick()
	{
		ClickAction?.Invoke(_data);
	}

	internal void Setup(StoryModeBattleDefinition data)
	{
		this._data = data;
		BattleImage.sprite = data.BattleImage;

		bool completed = Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CompletedLevels.Contains(data.LevelID);
		CompletedIndicator.SetActive(completed);
	}
}
