using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdventurePage : MonoBehaviour
{
	public VerticalDeckViewer DeckViewer;
	public GameObject Encouter;
	public GameObject EncouterChoices;
	public AdventureCardPicker CardPicker;

	public GameObject NewAdventurePrompt;

	private void Start()
	{
		//TODO make a better indicator for "start new game"
		var newAdventure = Common.Instance.SaveData.GameSaveData.AdventureSaveData.CurrentDeck == null;

		if (newAdventure)
		{
			NewAdventurePrompt.gameObject.SetActive(true);
			Encouter.gameObject.SetActive(false);
		}
		else
		{
			var deck = Common.Instance.SaveData.GameSaveData.AdventureSaveData.CurrentDeck.ToDeck();

			DeckViewer.Setup(deck);
			SetToEncounter();
		}
	}

	public void StartNewAdventure_Clicked()
	{
		var deck = Common.Instance.CardManager.AdventureStartDeck.ToDeck();
		Common.Instance.SaveData.GameSaveData.AdventureSaveData.CurrentDeck = DeckSaveData.FromDeck(deck);
		DeckViewer.Setup(deck);
		SetToEncounter();
	}

	private void SetToEncounter()
	{
		NewAdventurePrompt.gameObject.SetActive(false);

		Encouter.gameObject.SetActive(true);
		EncouterChoices.gameObject.SetActive(true);
		CardPicker.gameObject.SetActive(false);
	}

	public void GainCard_Clicked()
	{
		Encouter.gameObject.SetActive(true);
		EncouterChoices.gameObject.SetActive(false);
		CardPicker.gameObject.SetActive(true);
		CardPicker.Setup();
		CardPicker.CardPickedCallback = (cardDefinition) =>
		{
			DeckViewer.AddCardToDeck(cardDefinition);
			CardPicker.gameObject.SetActive(false);
			SetToEncounter();
		};
	}

	public void Battle_Clicked()
	{
		DeckSaveData deckSaveData = DeckSaveData.FromDeck(DeckViewer.GetDeck());
		Common.Instance.SaveData.GameSaveData.AdventureSaveData.CurrentDeck = deckSaveData;

		Common.Instance.SaveData.GameSaveData.CombatDeck = deckSaveData;
		Common.Instance.SaveData.GameSaveData.CombatDeckEnemy = 
			DeckSaveData.FromDeck(Common.Instance.CardManager.AdventureStartDeck.ToDeck());

		GameManager.ReturnScreenName = "Adventure";
		SceneManager.LoadScene("GameScene");
	}

	public void Rest_Clicked() { }
}
