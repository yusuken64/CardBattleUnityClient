using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenPackScene : MonoBehaviour
{
	public RectTransform PackArea;
	public FlippableCard FlippableCardPrefab;

	public float CardSpacing = 1.5f;
	public int CardCount = 5;
	public bool readyToOpen;

	public GameObject ClosedChest;
	public GameObject OpenChest;
	public GameObject OKButton;
	public GameObject ReturnButton;

	public int CardsOpend { get; private set; }

	public void Start()
	{
		ResetChest();
	}

	private static List<T> PickRandomWithReplacement<T>(IList<T> list, int count = 1)
	{
		if (list == null || list.Count == 0)
			throw new ArgumentException("PickRandom called with null or empty list");

		var result = new List<T>(count);
		for (int i = 0; i < count; i++)
			result.Add(list[UnityEngine.Random.Range(0, list.Count)]);

		return result;
	}

	private void ClearPack()
	{
		foreach(Transform child in PackArea)
		{
			Destroy(child.gameObject);
		}
	}

	public void OpenPack()
	{
		if (!readyToOpen)
		{
			return;
		}

		ClosedChest.gameObject.SetActive(false);
		OpenChest.gameObject.SetActive(true);
		ReturnButton.gameObject.SetActive(false);
		readyToOpen = false;

		float totalWidth = (CardCount - 1) * CardSpacing;
		float startX = -totalWidth * 0.5f;

		for (int i = 0;i < CardCount; i++)
		{
			Vector3 localPos = new Vector3(
				startX + i * CardSpacing,
				0f,
				0f
			);

			FlippableCard card = Instantiate(
				FlippableCardPrefab,
				PackArea
			);

			CardDefinition cardDefinition = PickRandomWithReplacement(Common.Instance.CardManager.CollectableCards())[0];

			card.transform.localPosition = localPos;
			card.transform.localRotation = Quaternion.identity;
			card.transform.localScale = Vector3.one;
			card.FlipComplete = FlipComplete;
			card.Setup(cardDefinition);
		}
	}

	private void FlipComplete()
	{
		CardsOpend++;

		if (CardsOpend >= CardCount)
		{
			OKButton.gameObject.SetActive(true);
		}
	}

	public void OK_Clicked()
	{
		ResetChest();
	}

	private void ResetChest()
	{
		readyToOpen = true;
		ClearPack();
		ClosedChest.gameObject.SetActive(true);
		OpenChest.gameObject.SetActive(false);
		OKButton.gameObject.SetActive(false);
		ReturnButton.gameObject.SetActive(true);
		CardsOpend = 0;
	}

	public void Return_Clicked()
	{
		SceneManager.LoadScene("Main");
	}
}
