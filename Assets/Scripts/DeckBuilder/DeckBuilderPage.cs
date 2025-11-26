using UnityEngine;
using UnityEngine.SceneManagement;

public class DeckBuilderPage : MonoBehaviour
{
	public DeckPicker DeckPicker;
	public VerticalDeckViewer DeckViewer;

	private void Start()
	{
		DeckPicker.gameObject.SetActive(true);
		DeckViewer.gameObject.SetActive(false);

		DeckPicker.DeckPickedAction = DeckPicker_DeckPicked;
		DeckViewer.DeckClosedAction = DeckViewer_DeckSaved;
	}

	public void DeckPicker_DeckPicked(DeckPickerButton deckPickerButton)
	{
		DeckPicker.gameObject.SetActive(false);
		DeckViewer.gameObject.SetActive(true);
		DeckViewer.Setup(deckPickerButton.Deck);
	}

	public void DeckViewer_DeckSaved(Deck deckData)
	{
		if (deckData != null)
		{
			//Update data?
			Common.Instance.SaveManager.Save(Common.Instance.GameSaveData);
		}
		DeckPicker.gameObject.SetActive(true);
		DeckPicker.UpdateUI();
		DeckViewer.gameObject.SetActive(false);
	}

	public void ReturnToMain()
	{
		SceneManager.LoadScene("Main");
	}
}
