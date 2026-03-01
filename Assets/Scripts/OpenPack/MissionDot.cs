using System.Linq;
using TMPro;
using UnityEngine;

public class MissionDot : MonoBehaviour
{
	public TextMeshProUGUI CountText;

	private void Start()
	{
		RefreshData();
	}

    public void RefreshData()
    {
        var saveData = Common.Instance.SaveManager.SaveData.GameSaveData;
        var questTracker = Common.Instance.QuestTracker;

        var progressById = saveData.QuestSaveData.QuestProgressList
            .ToDictionary(p => p.questId);

        int collectableCount = questTracker.QuestDefinitions
            .Where(q => progressById.TryGetValue(q.QuestId, out var progress)
                        && progress.questProgress >= q.MaxProgress
                        && !progress.Collected)
            .Count();

        gameObject.SetActive(collectableCount > 0);
        CountText.text = collectableCount.ToString();
    }
}