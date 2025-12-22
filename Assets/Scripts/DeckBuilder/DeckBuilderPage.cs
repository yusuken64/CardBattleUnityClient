using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckBuilderPage : MonoBehaviour
{
	public DeckPicker DeckPicker;
	public VerticalDeckViewer DeckViewer;
	public Collection Collection;

	private void Start()
	{
		DeckPicker.gameObject.SetActive(true);
		DeckViewer.gameObject.SetActive(false);

		DeckPicker.DeckPickedAction = DeckPicker_DeckPicked;
		DeckViewer.DeckClosedAction = DeckViewer_DeckSaved;
		Collection.SetToCollectionView();
	}

	public void DeckPicker_DeckPicked(DeckPickerButton deckPickerButton)
	{
		DeckPicker.gameObject.SetActive(false);
		DeckViewer.gameObject.SetActive(true);
		DeckViewer.DeckChanged = DeckViewer_DeckChanged;
		DeckViewer.Setup(deckPickerButton.Deck);
	}

	private void DeckViewer_DeckChanged(Deck deck)
	{
		Collection.SetToDeckView(deck);
	}

	public void DeckViewer_DeckSaved(Deck deck)
	{
		if (deck != null)
		{
			//Update data?
			Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas[0] = DeckSaveData.FromDeck(deck);
			Common.Instance.SaveManager.Save();
			var button = DeckPicker.DeckPickerButtons.FirstOrDefault(x => x.Deck == deck);
			button.Setup(Common.Instance.SaveManager.SaveData.GameSaveData.DeckSaveDatas[0]);
		}
		DeckPicker.gameObject.SetActive(true);
		DeckPicker.UpdateUI();
		DeckViewer.gameObject.SetActive(false);
		Collection.SetToCollectionView();
	}

	public void ReturnToMain()
	{
		SceneManager.LoadScene("Main");
	}
}
