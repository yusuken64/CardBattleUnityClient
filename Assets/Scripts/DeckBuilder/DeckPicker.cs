using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckPicker : MonoBehaviour
{
	public DeckPickerButton DeckPickerButtonPrefab;
	public Transform DeckPickerButtonContainer;
	public List<DeckPickerButton> DeckPickerButtons;

	public Action<DeckPickerButton> DeckPickedAction { get; internal set; }

	private void Start()
	{
		foreach (Transform transform in DeckPickerButtonContainer)
		{
			Destroy(transform.gameObject);
		}
		DeckPickerButtons.Clear();

		foreach (var deck in Common.Instance.GameSaveData.Decks)
		{
			var deckPickerButton = Instantiate(DeckPickerButtonPrefab, DeckPickerButtonContainer);
			deckPickerButton.DeckPickedAction = DeckPickedAction;
			deckPickerButton.Setup(deck);
			DeckPickerButtons.Add(deckPickerButton);
		}
		UpdateUI();
	}

	public void UpdateUI()
	{
		foreach(var deckPickerButton in DeckPickerButtons)
		{
			deckPickerButton.UpdateUI();
		}
	}
}
