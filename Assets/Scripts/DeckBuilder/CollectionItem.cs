using System;
using UnityEngine;

public class CollectionItem : MonoBehaviour, IClickable
{
	public Card Card;
	public CardDefinition CardDefinition;

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
}
