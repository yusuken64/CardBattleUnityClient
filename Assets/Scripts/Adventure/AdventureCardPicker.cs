using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdventureCardPicker : MonoBehaviour
{
	public Transform Container;
	public CardPickerItem ItemPrefab;

	public Action<CardDefinition> CardPickedCallback;
	public KeywordDetailList KeywordDetailList;

	public void Setup(bool isHeroSet = true)
	{
		KeywordDetailList.gameObject.SetActive(false);

		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}

		List<CardDefinition> allCards = Common.Instance.CardManager.CollectableCards();
		if (!isHeroSet)
		{
			allCards.RemoveAll(x => x is not MinionCardDefinition);
		}

		//randomly take 3 cards
		var choices = allCards
			.OrderBy(_ => UnityEngine.Random.Range(0, int.MaxValue))
			.Take(3)
			.ToList();

		foreach(var choice in choices)
		{
			var newItem = Instantiate(ItemPrefab, Container);
			newItem.Setup(choice);
			newItem.CardPickedCallBack = ChoiceClicked;
			newItem.CardPickerHoverCallback = OnCardPickerHover;
		}
	}

	public void OnCardPickerHover(CardPickerItem cardPickerItem)
	{
		if (cardPickerItem == null)
		{
			KeywordDetailList.gameObject.SetActive(false);
			return;
		}

		KeywordDetailList.gameObject.SetActive(true);
		KeywordDetailList.transform.position = cardPickerItem.GetPosition();
		KeywordDetailList.Setup(cardPickerItem.Card);
	}

	public void ChoiceClicked(CardDefinition cardChoice)
	{
		CardPickedCallback?.Invoke(cardChoice);
	}
}
