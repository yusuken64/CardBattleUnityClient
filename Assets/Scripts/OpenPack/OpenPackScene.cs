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
	public PackRedDot PackRedDot;

	public int CardsOpened { get; private set; }
	public List<CardDefinition> CardsCollected = new();

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
		CardsCollected.Clear();
	}

	public void OpenPack()
	{
		if (!readyToOpen)
		{
			return;
		}

		var packCount = Common.Instance.SaveManager.SaveData.GameSaveData.PackCount;
		if (packCount <= 0)
		{
			return;
		}

		Common.Instance.SaveManager.SaveData.GameSaveData.PackCount -= 1;

		CardsCollected.Clear();

		ClosedChest.gameObject.SetActive(false);
		OpenChest.gameObject.SetActive(true);
		ReturnButton.gameObject.SetActive(false);
		readyToOpen = false;

		float totalWidth = (CardCount - 1) * CardSpacing;
		float startX = -totalWidth * 0.5f;

		for (int i = 0; i < CardCount; i++)
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

			CardsCollected.Add(cardDefinition);
		}
	}

	private void FlipComplete()
	{
		CardsOpened++;

		if (CardsOpened >= CardCount)
		{
			OKButton.gameObject.SetActive(true);
		}
	}

	public void OK_Clicked()
	{
		foreach(var card in CardsCollected)
		{
			Common.Instance.SaveManager.SaveData.GameSaveData.CardCollection.Add(card.CardName);
		}

		ResetChest();
	}

	private void ResetChest()
	{
		PackRedDot.RefreshData();
		readyToOpen = true;
		ClearPack();
		ClosedChest.gameObject.SetActive(true);
		OpenChest.gameObject.SetActive(false);
		OKButton.gameObject.SetActive(false);
		ReturnButton.gameObject.SetActive(true);
		CardsOpened = 0;
	}

	public void Return_Clicked()
	{
		SceneManager.LoadScene("Main");
	}
}
