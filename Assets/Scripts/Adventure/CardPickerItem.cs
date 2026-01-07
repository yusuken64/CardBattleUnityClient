using System;
using UnityEngine;

public class CardPickerItem : MonoBehaviour
{
	public Card Card;
	public CardDefinition CardDefinition;

	public Action<CardDefinition> CardPickedCallBack;

	public void Setup(CardDefinition cardDefinition)
	{
		this.CardDefinition = cardDefinition;
		CardBattleEngine.Card cardData = cardDefinition.CreateCard();
		if (cardData == null) { return; }
		//try
		//{
		//	cardData.Owner = owner;
		//}
		//catch (Exception exception)
		//{
		//	Debug.LogException(exception);
		//}
		Card.Setup(cardData);
	}

	public void Card_Clicked()
	{
		CardPickedCallBack?.Invoke(CardDefinition);
	}
}
