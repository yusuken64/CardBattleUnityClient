using System;
using UnityEngine;

public class CollectionItem : MonoBehaviour, IClickable, IHoverable
{
	public Card Card;
	public CardDefinition CardDefinition;
	public HoldClickableButton HoldClickableButton;

	public CanvasGroup CanvasGroup;

	private void OnEnable()
	{
		HoldClickableButton.OnHoldClicked += OnHoldClicked;
	}

	private void OnDisable()
	{
		HoldClickableButton.OnHoldClicked -= OnHoldClicked;
	}

	public void Setup(CardDefinition cardDefinition, CardBattleEngine.Player owner)
	{
		this.CardDefinition = cardDefinition;
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
		verticalDeckViewer?.AddCardToDeck(CardDefinition);
	}

	public CardBattleEngine.Card GetDisplayCard()
	{
		return CardDefinition.CreateCard();
	}

	public void HoldStart()
	{
		var ui = FindFirstObjectByType<CollectionUI>();
		ui.PreviewStart(this);
	}

	public void HoldEnd()
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
}
