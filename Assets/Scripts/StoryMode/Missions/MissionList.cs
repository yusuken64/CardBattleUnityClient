using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionList : MonoBehaviour
{
	public StoryModeScene StoryModeScene;
	public Transform Container;
	public MissionListItem MissionListItemPrefab;
	public List<MissionListItem> MissionListItems;

	public MissionDetails MissionDetails;

	public void Setup()
	{
		foreach(Transform child in Container)
		{
			Destroy(child.gameObject);
		}
		MissionListItems.Clear();

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var progressLookup = questData.QuestProgressList.ToDictionary(x => x.questId);

		var allQuests = Common.Instance.QuestTracker.QuestDefinitions
			.Select(q =>
			{
				progressLookup.TryGetValue(q.QuestId, out var progress);

				bool isCollected = progress?.Collected ?? false;
				bool isAccepted = progress != null;
				float percent = progress == null || q.MaxProgres == 0
					? 0f
					: progress.questProgress / (float)q.MaxProgres;

				int bucket = isCollected ? 2 : (isAccepted ? 0 : 1);

				return new
				{
					Quest = q,
					Bucket = bucket,
					Percent = percent
				};
			})
			.OrderBy(x => x.Bucket)
			.ThenByDescending(x => x.Percent)
			.Select(x => x.Quest)
			.ToList();

		foreach (var quest in allQuests)
		{
			var newItem = Instantiate(MissionListItemPrefab, Container);
			newItem.Setup(quest);
			newItem.ClickCallBack = MissionListItem_Clicked;
			MissionListItems.Add(newItem);
		}

		MissionListItem_Clicked(allQuests[0]);
	}

	private void MissionListItem_Clicked(QuestDefinition obj)
	{
		foreach(var item in MissionListItems)
		{
			item.SetFocus(obj == item.Quest);
		}
		MissionDetails.Setup(obj);
	}

	public void Back_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			StoryModeScene.RefreshRedDots();
			gameObject.SetActive(false);
		});
	}
}
