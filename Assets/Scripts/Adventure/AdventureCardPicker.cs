using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AdventureCardPicker : MonoBehaviour
{
	public Transform Container;
	public CardPickerItem ItemPrefab;

	public Action<CardDefinition> CardPickedCallback;

	public void Setup()
	{
		foreach(Transform child in Container)
		{
			Destroy(child.gameObject);
		}

		List<CardDefinition> allCards = Common.Instance.CardManager.CollectableCards();
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
		}
	}

	public void ChoiceClicked(CardDefinition cardChoice)
	{
		CardPickedCallback?.Invoke(cardChoice);
	}
}
