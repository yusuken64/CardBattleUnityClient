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
		PreviewCard.Setup(collectionItem.GetDisplayCard());
	}

	public void PreviewEnd()
	{
		PreviewObject.SetActive(false);
	}
}
