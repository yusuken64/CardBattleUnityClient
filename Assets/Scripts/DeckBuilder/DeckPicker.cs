using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeckPicker : MonoBehaviour
{
	public DeckPickerButton DeckPickerButtonPrefab;
	public Transform DeckPickerButtonContainer;
	public List<DeckPickerButton> DeckPickerButtons;
	public Button AddDeckButtonPrefab;

	public Action<DeckPickerButton> DeckPickedAction { get; internal set; }
	public Action<DeckPickerButton> DeckSelectedAction { get; internal set; }
	public Action<DeckPickerButton> DeleteRequestedAction { get; internal set; }
	public bool SelectMode;
	public int DeckCountMax;


    private void Start()
	{
		Rebuild();
	}

	public void Rebuild()
	{
		foreach (Transform transform in DeckPickerButtonContainer)
		{
			Destroy(transform.gameObject);
		}
		DeckPickerButtons.Clear();

		var activeDeckID = Common.Instance.SaveManager.SaveData.GameSaveData.ActiveDeckID;

		foreach (var deckData in Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas)
		{
			var deckPickerButton = Instantiate(DeckPickerButtonPrefab, DeckPickerButtonContainer);
			deckPickerButton.DeckPickedAction = DeckPickedAction;
			deckPickerButton.DeckSelectedAction = DeckSelectedAction;
			deckPickerButton.DeleteRequestedAction = DeleteRequestedAction;
			deckPickerButton.SelectMode = SelectMode;

			deckPickerButton.Setup(deckData);
			deckPickerButton.SetActiveHighlight(deckData.ID == activeDeckID);
			DeckPickerButtons.Add(deckPickerButton);
		}

		var deckCount = Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas.Count();

        if (!SelectMode && deckCount < DeckCountMax)
		{
			var addDeckButton = Instantiate(AddDeckButtonPrefab, DeckPickerButtonContainer);
			addDeckButton.GetComponent<Button>().onClick.AddListener(() => 
			{
				AddDeck_Clicked();
            });
		}

		UpdateUI();
	}

	public void AddDeck_Clicked()
	{
		var newDeckData = new DeckSaveData
		{
			ID = Guid.NewGuid().ToString(),
			Title = "New Deck",
			CardIDs = new List<string>()
		};

		Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas.Add(newDeckData);
		Common.Instance.SaveManager.Save();

		Rebuild();

		//var newButton = DeckPickerButtons.FirstOrDefault(b => b.DeckID == newDeckData.ID);
		//if (newButton != null)
		//	DeckPickedAction?.Invoke(newButton);
	}

	public void UpdateUI()
	{
		foreach(var deckPickerButton in DeckPickerButtons)
		{
			deckPickerButton.UpdateUI();
		}
	}
}
