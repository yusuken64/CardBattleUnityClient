using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionListItem : MonoBehaviour
{
	public TextMeshProUGUI MissionTitle;
	public TextMeshProUGUI StatusText;
	public Image Background;
	public Image Focus;
	public QuestDefinition Quest;

	public Action<QuestDefinition> ClickCallBack;

	internal void Setup(QuestDefinition quest)
	{
		this.Quest = quest;
		UpdateUI();
	}

	public void UpdateUI()
	{
		MissionTitle.text = Quest.QuestTitle;

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var progress = questData.QuestProgressList
			.FirstOrDefault(x => x.questId == Quest.QuestId);

		var hasProgress = progress != null;
		StatusText.gameObject.SetActive(hasProgress);

		if (!hasProgress)
			return;

		StatusText.text = progress.Collected
			? "Complete"
			: $"{progress.questProgress}/{Quest.MaxProgress}";
	}

	internal void Select()
	{
		SetFocus(true);
	}

	public void SetFocus(bool focus)
	{
		Focus.gameObject.SetActive(focus);
	}

	public void Click()
	{
		ClickCallBack?.Invoke(Quest);
	}

	[ContextMenu("SetToMax")]
	public void SetToMax()
	{
		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var progress = questData.QuestProgressList
			.FirstOrDefault(x => x.questId == Quest.QuestId);
		progress.questProgress = Quest.MaxProgress;
		UpdateUI();
	}

	[ContextMenu("Reset")]
	public void SetReset()
	{
		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var progress = questData.QuestProgressList
			.FirstOrDefault(x => x.questId == Quest.QuestId);
		questData.QuestProgressList.Remove(progress);
		UpdateUI();
	}
}
