using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardPickerItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public Card Card;
	public CardDefinition CardDefinition;

	public Action<CardDefinition> CardPickedCallBack;
	public Vector3 ToolTipOffset;

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
		HoverEnd();
		CardPickedCallBack?.Invoke(CardDefinition);
	}

	public Action<CardPickerItem> CardPickerHoverCallback { get; internal set; }

	public void HoverStart()
	{
		CardPickerHoverCallback?.Invoke(this);
	}

	public void HoverEnd()
	{
		CardPickerHoverCallback?.Invoke(null);
	}

	public Vector3 GetPosition()
	{
		return this.transform.position + ToolTipOffset;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		HoverStart();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HoverEnd();
	}
}
