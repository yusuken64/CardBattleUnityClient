using System;
using UnityEngine;

public class CollectionUI : MonoBehaviour
{
	public GameObject PreviewObject;
	public Card PreviewCard;

	private void Start()
	{
		PreviewObject.SetActive(false);
	}

	internal void PreviewStart(CollectionItem collectionItem)
	{
		PreviewObject.SetActive(true);
		PreviewCard.Setup(collectionItem.DisplayCard);
	}

	internal void PreviewMove(CollectionItem collectionItem)
	{
		//throw new NotImplementedException();
	}

	internal void PreviewStart(CardDefinition cardDefinition)
	{
		PreviewObject.SetActive(true);
		PreviewCard.Setup(cardDefinition.CreateCard());
	}


	public void PreviewEnd()
	{
		PreviewObject.SetActive(false);
	}
}
