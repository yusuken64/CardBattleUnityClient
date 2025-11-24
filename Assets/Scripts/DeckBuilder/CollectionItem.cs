using UnityEngine;

public class CollectionItem : MonoBehaviour, IClickable
{
	public Card Card;
	public CardDefinition CardDefinition;

	public void Setup(CardDefinition cardDefinition, CardBattleEngine.Player owner)
	{
		this.CardDefinition = cardDefinition;
		CardBattleEngine.Card cardData = cardDefinition.CreateCard();
		cardData.Owner = owner;
		Card.Setup(cardData);
	}
	public void OnClick()
	{
		var verticalDeckViewer = FindFirstObjectByType<VerticalDeckViewer>();
		verticalDeckViewer?.AddCardToDeck(CardDefinition);
	}
}
