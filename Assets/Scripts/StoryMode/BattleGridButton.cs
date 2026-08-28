using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleGridButton : MonoBehaviour
{
	public TextMeshProUGUI NameText;
	public Image BattleImage;
	public GameObject CompletedIndicator;

	private StoryModeDungeonDefinition _data;

	public Action<StoryModeDungeonDefinition> ClickAction;

	public bool IsComplete;
	public CanvasGroup CanvasGroup;

	public void OnClick()
	{
		ClickAction?.Invoke(_data);
	}

	internal void Setup(StoryModeDungeonDefinition data)
	{
		this._data = data;
		NameText.text = data.DungeonName;

		IsComplete = Common.Instance.SaveManager.SaveData.GameSaveData.StorySaveData.CompletedLevels.Contains(data.DungeonID);
		CompletedIndicator.SetActive(IsComplete);
	}
}
