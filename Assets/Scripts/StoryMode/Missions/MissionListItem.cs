using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionListItem : MonoBehaviour
{
	public TextMeshProUGUI MissionTitle;
	public Image Background;
	public Image Focus;
	public QuestDefinition Quest;

	public Action<QuestDefinition> ClickCallBack;

	internal void Setup(QuestDefinition quest)
	{
		this.Quest = quest;
		MissionTitle.text = quest.QuestTitle;
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
}
