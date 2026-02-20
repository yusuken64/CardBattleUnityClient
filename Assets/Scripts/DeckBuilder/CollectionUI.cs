using System;
using UnityEngine;

public class CollectionUI : MonoBehaviour
{
	public GameObject PreviewObject;
	public Card PreviewCard;

	public KeywordDetailList KeywordDetailList;

	private void Start()
	{
		PreviewObject.SetActive(false);
		KeywordDetailList.gameObject.SetActive(false);
	}

	internal void PreviewStart(CollectionItem collectionItem)
	{
		PreviewObject.SetActive(true);
		PreviewCard.Setup(collectionItem.DisplayCard);

		KeywordDetailList.gameObject.SetActive(true);
		KeywordDetailList.Setup(collectionItem.Card);
	}

	internal void PreviewMove(CollectionItem collectionItem)
	{
		//throw new NotImplementedException();
	}

	public void PreviewEnd()
	{
		PreviewObject.SetActive(false);
		KeywordDetailList.gameObject.SetActive(false);
	}
}
