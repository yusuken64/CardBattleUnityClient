using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdventurePage : MonoBehaviour
{
	public VerticalDeckViewer DeckViewer;
	public GameObject Encouter;
	public GameObject EncouterChoices;
	public AdventureCardPicker CardPicker;

	private void Start()
	{
		//var deck = Common.Instance.SaveData.GameSaveData.AdventureSaveData.CurrentDeck.ToDeck();
		Deck deck = new Deck();
		deck.Title = "Test";
		deck.HeroCard = Common.Instance.CardManager.AllCards()
			.OrderBy(x => UnityEngine.Random.Range(0, int.MaxValue))
			.First();
		deck.Cards = new();
		DeckViewer.Setup(deck);
		SetToEncounter();
	}

	private void SetToEncounter()
	{
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
		DeckSaveData newDeck = new();
		var deckSaveData = newDeck.FromDeck(DeckViewer.GetDeck());
		Common.Instance.SaveData.GameSaveData.CombatDeck = deckSaveData;
		SceneManager.LoadScene("GameScene");
	}

	public void Rest_Clicked() { }
}
