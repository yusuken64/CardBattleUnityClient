using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class CollectionItem : MonoBehaviour, IClickable, IHoverable
{
	public Card Card;
	public CardDefinition CardDefinition;
	public OwnedCardData OwnedCardData;

	public HoldClickableButton HoldClickableButton;

	public CanvasGroup CanvasGroup;

	public GameObject NoCardsLeftIndicator;
	public GameObject CountObject;
	public TextMeshProUGUI CountText;

	private void OnEnable()
	{
		HoldClickableButton.OnHoldClicked += OnHoldClicked;
	}

	private void OnDisable()
	{
		HoldClickableButton.OnHoldClicked -= OnHoldClicked;
	}

	public void Setup(CardDefinition cardDefinition, CardBattleEngine.Player owner, OwnedCardData ownedCardData)
	{
		this.CardDefinition = cardDefinition;
		this.OwnedCardData = ownedCardData;
		CardBattleEngine.Card cardData = cardDefinition.CreateCard();
		if (cardData == null) { return; }
		try
		{
			cardData.Owner = owner;
		}
		catch(Exception exception)
		{
			Debug.LogException(exception);
		}
		Card.Setup(cardData);
	}

	public void OnClick()
	{
		var verticalDeckViewer = FindFirstObjectByType<VerticalDeckViewer>();
		if (verticalDeckViewer == null) { return; }

		var deck = verticalDeckViewer.GetDeck();
		var usedCount = deck.Cards.Count(x => x.CardName == CardDefinition.CardName);
		if (usedCount < OwnedCardData.Count)
		{
			verticalDeckViewer?.AddCardToDeck(CardDefinition, false);
		}
	}

	public CardBattleEngine.Card GetDisplayCard()
	{
		return CardDefinition.CreateCard();
	}

	public void HoverStart()
	{
		var ui = FindFirstObjectByType<CollectionUI>();
		ui.PreviewStart(this);
	}

	public void HoverEnd()
	{
		var ui = FindFirstObjectByType<CollectionUI>();
		ui.PreviewEnd();
	}

	private void OnHoldClicked()
	{
		var ui = FindFirstObjectByType<CollectionUI>(FindObjectsInactive.Include);
		ui.gameObject.SetActive(true);
		ui.PreviewStart(this);
	}

	public void SetToCollected()
	{
		CanvasGroup.alpha = 1;
	}

	public void SetToUnCollected()
	{
		CanvasGroup.alpha = 0.2f;
	}

	public void SetToCollectionView()
	{
		NoCardsLeftIndicator.gameObject.SetActive(false);
		CountObject.gameObject.SetActive(true);
		CountText.text = OwnedCardData.Count.ToString();
	}

	public void SetToDeckView(Deck deck)
	{
		var usedCount = deck.Cards.Count(x => x.CardName == CardDefinition.CardName);
		NoCardsLeftIndicator.gameObject.SetActive(usedCount >= OwnedCardData.Count);
		CountObject.gameObject.SetActive(true);
		CountText.text = (OwnedCardData.Count - usedCount).ToString();
	}
}
