using System.Collections.Generic;
using UnityEngine;

public class ModCardPreviews : MonoBehaviour
{
	public GameObject ModCardPreviewItem;
	public Transform ModCardPreviewContainer;

	public void Setup(List<CardDefinition> data)
	{
		foreach(Transform child in ModCardPreviewContainer)
		{
			Destroy(child.gameObject);
		}

		foreach(var item in data)
		{
			var newItem = Instantiate(ModCardPreviewItem, ModCardPreviewContainer);
			var card = newItem.GetComponentInChildren<Card>();
			card.Setup(item.CreateCard());
		}
	}
}
