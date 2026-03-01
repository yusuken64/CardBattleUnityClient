using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MissionList : MonoBehaviour
{
	public StoryModeScene StoryModeScene;
	public Transform Container;
	public MissionListItem MissionListItemPrefab;
	public List<MissionListItem> MissionListItems;

	public MissionDetails MissionDetails;
	public TextMeshProUGUI ActiveMissionCountText;

	public void Setup()
	{
		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}
		MissionListItems.Clear();

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var progressLookup = questData.QuestProgressList.ToDictionary(x => x.questId);

		var allQuests = Common.Instance.QuestTracker.QuestDefinitions
			.Where(x => QuestTracker.IsQuestUnlocked(x))
			.Select(q =>
			{
				progressLookup.TryGetValue(q.QuestId, out var progress);

				bool isCollected = progress?.Collected ?? false;
				bool isAccepted = progress != null;
				float percent = progress == null || q.MaxProgress == 0
					? 0f
					: progress.questProgress / (float)q.MaxProgress;

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

		if (allQuests.Count > 0)
		{
			MissionListItem_Clicked(allQuests[0]);
		}
	}

	public void Reorder()
	{
		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		Dictionary<string, QuestProgress> progressLookup = questData.QuestProgressList
			.ToDictionary(x => x.questId);

		MissionListItems.Sort((a, b) =>
		{
			var stateA = GetQuestSortState(a.Quest, progressLookup);
			var stateB = GetQuestSortState(b.Quest, progressLookup);

			// 1. Bucket comparison
			int bucketCompare = stateA.Bucket.CompareTo(stateB.Bucket);
			if (bucketCompare != 0)
				return bucketCompare;

			// 2. Percent descending
			return stateB.Percent.CompareTo(stateA.Percent);
		});

		// Apply new visual order
		for (int i = 0; i < MissionListItems.Count; i++)
		{
			MissionListItems[i].transform.SetSiblingIndex(i);
		}
	}

	private enum QuestOrderBucket
	{
		Active = 0,
		Inactive = 1,
		Collected = 2
	}

	private (QuestOrderBucket Bucket, float Percent)
		GetQuestSortState(
			QuestDefinition q,
			Dictionary<string, QuestProgress> lookup)
	{
		lookup.TryGetValue(q.QuestId, out var progress);

		bool isCollected = progress?.Collected ?? false;
		bool isActive = progress != null && !isCollected;

		float percent = (progress == null || q.MaxProgress == 0)
			? 0f
			: progress.questProgress / (float)q.MaxProgress;

		var bucket = isCollected
			? QuestOrderBucket.Collected
			: (isActive ? QuestOrderBucket.Active : QuestOrderBucket.Inactive);

		return (bucket, percent);
	}

	private void MissionListItem_Clicked(QuestDefinition obj)
	{
		foreach (var item in MissionListItems)
		{
			item.SetFocus(obj == item.Quest);
		}
		MissionDetails.Setup(obj);
		MissionDetails.MissionChanged = MissionChanged;
		RefreshUI();
	}

	private void MissionChanged()
	{
		Reorder();
		MissionListItems.ForEach(x => x.UpdateUI());
		RefreshUI();
	}

	public void RefreshUI()
	{
		int inProgressCount = GetActiveMissionsCount();
		int maxActiveMissions = 3;
		ActiveMissionCountText.text = $"Active Missions {inProgressCount}/{maxActiveMissions}";
	}

	public static int GetActiveMissionsCount()
	{
		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		var inProgressCount = questData.QuestProgressList.Where(x => !x.Collected).Count();
		return inProgressCount;
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