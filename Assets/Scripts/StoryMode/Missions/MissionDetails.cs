using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionDetails : MonoBehaviour
{
	public TextMeshProUGUI Title;
	public TextMeshProUGUI Description;

	public Slider ProgressSlider;
	public TextMeshProUGUI ProgressText;
	public TextMeshProUGUI ObjectiveText;

	public GameObject ButtonContainer;
	public Button AcceptButton;
	public Button CompleteButton;
	public Button AbortButton;
	public GameObject CompleteIndicator;

	private QuestDefinition _currentQuest;
	private QuestProgress _currentProgress;

	public Action MissionChanged;

	public void Setup(QuestDefinition obj)
	{
		_currentQuest = obj;

		Title.text = obj.QuestTitle;
		Description.text = obj.QuestDescription;

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		_currentProgress = questData.QuestProgressList
			.FirstOrDefault(x => x.questId == obj.QuestId);

		if (_currentProgress == null)
		{
			SetToUnAccepted();
		}
		else if (_currentProgress.Collected)
		{
			SetToCompleted();
		}
		else
		{
			SetToInProgress();
		}

		RefreshUI();
	}

	private void RefreshUI()
	{
		ObjectiveText.text = $"{_currentQuest.QuestObjective}";
		ProgressText.text = $"{ProgressSlider.value}/{ProgressSlider.maxValue}";
	}

	private void SetToUnAccepted()
	{
		ProgressSlider.maxValue = _currentQuest.MaxProgress;
		ProgressSlider.value = 0;

		ButtonContainer.gameObject.SetActive(true);
		CompleteIndicator.gameObject.SetActive(false);

		AcceptButton.gameObject.SetActive(true);
		if (MissionList.GetActiveMissionsCount() >= 3)
		{
			AcceptButton.interactable = false;
		}
		else
		{
			AcceptButton.interactable = true;
		}

		AcceptButton.gameObject.SetActive(true);
		CompleteButton.gameObject.SetActive(false);
		AbortButton.gameObject.SetActive(false);
	}

	private void SetToInProgress()
	{
		ProgressSlider.maxValue = _currentQuest.MaxProgress;
		ProgressSlider.value = _currentProgress.questProgress;

		ButtonContainer.gameObject.SetActive(true);
		CompleteIndicator.gameObject.SetActive(false);
		AcceptButton.gameObject.SetActive(false);

		bool isComplete = _currentProgress.questProgress >= _currentQuest.MaxProgress;
		CompleteButton.interactable = isComplete;
		AbortButton.interactable = !isComplete;
		
		CompleteButton.gameObject.SetActive(true);
		AbortButton.gameObject.SetActive(true);
	}

	private void SetToCompleted()
	{
		ProgressSlider.maxValue = _currentQuest.MaxProgress;
		ProgressSlider.value = _currentProgress.questProgress;

		ButtonContainer.gameObject.SetActive(false);
		CompleteIndicator.gameObject.SetActive(true);
		AcceptButton.gameObject.SetActive(false);
		CompleteButton.gameObject.SetActive(false);
		AbortButton.gameObject.SetActive(false);
	}

	public void Accept_Click()
	{
		if (_currentQuest == null)
			return;

		if (MissionList.GetActiveMissionsCount() >= 3)
		{
			return;
		}

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;

		_currentProgress = new QuestProgress
		{
			questId = _currentQuest.QuestId,
			questProgress = 0
		};

		questData.QuestProgressList.Add(_currentProgress);

		SetToInProgress();
		RefreshUI();

		Common.Instance.SaveManager.Save();
		MissionChanged?.Invoke();
	}

	public void Complete_Click()
	{
		if (_currentQuest == null || _currentProgress == null)
			return;

		if (_currentProgress.questProgress < _currentQuest.MaxProgress)
			return;

		_currentProgress.Collected = true;

		SetToCompleted();
		RefreshUI();

		Common.Instance.SaveManager.SaveData.GameSaveData.PackCount++;
		Common.Instance.SaveManager.Save();
		MissionChanged?.Invoke();
	}

	public void Abort_Click()
	{
		if (_currentQuest == null || _currentProgress == null)
			return;

		var questData = Common.Instance.SaveManager.SaveData.GameSaveData.QuestSaveData;
		questData.QuestProgressList.Remove(_currentProgress);

		_currentProgress = null;

		SetToUnAccepted();
		RefreshUI();

		Common.Instance.SaveManager.Save();
		MissionChanged?.Invoke();
	}
}
