using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OpenPackScene : MonoBehaviour
{
	public RectTransform PackArea;
	public FlippableCard FlippableCardPrefab;

	public float CardSpacing = 1.5f;
	public int CardCount = 5;
	public bool readyToOpen;

	public Transform ChestParent;
	public GameObject ClosedChest;
	public GameObject OpenChest;
	public GameObject OKButton;
	public GameObject ReturnButton;
	public PackRedDot PackRedDot;

	public int CardsOpened { get; private set; }
	public List<CardDefinition> CardsCollected = new();

	public static string ReturnScreenName;

	public void Start()
	{
		ResetChest();
	}

	public static List<T> PickRandomWithReplacement<T>(IList<T> list, int count = 1)
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
		if (!CanOpenPack())
		{
			return;
		}

		ClosedChest.GetComponent<Button>().interactable = false;
		CardsCollected.Clear();

		ReturnButton.gameObject.SetActive(false);
		StartCoroutine(OpenPackRoutine());
	}

	public bool CanOpenPack()
	{
		if (!readyToOpen)
		{
			return false;
		}

		var packCount = Common.Instance.SaveManager.SaveData.GameSaveData.PackCount;
		if (packCount <= 0)
		{
			return false;
		}

		return true;
	}

	private IEnumerator OpenPackRoutine()
	{
		yield return null;
		readyToOpen = false;
		Common.Instance.SaveManager.SaveData.GameSaveData.PackCount -= 1;

		ClosedChest.gameObject.SetActive(true);
		OpenChest.gameObject.SetActive(false);
		float cascadeDelay = 0.3f;
		float totalWidth = (CardCount - 1) * CardSpacing;
		float startX = -totalWidth * 0.5f;

		Vector3 chestPos = ChestParent.position;

		List<CardDefinition> CardDefinitions = new();
		for (int i = 0; i < CardCount; i++)
		{
			CardDefinition cardDefinition = PickRandomWithReplacement(Common.Instance.CardManager.CollectableCards())[0];
			CardDefinitions.Add(cardDefinition);
			CardsCollected.Add(cardDefinition);
		}

		Sequence chestSequence = DOTween.Sequence();

		// 1. Shake
		chestSequence.Append(ChestParent.DOShakePosition(0.5f, 1f, 20, 90, false, true));

		// 2. Shrink
		chestSequence.Append(ChestParent.DOScale(0.3f, 0.4f).SetEase(Ease.InBack));

		// 3. Pop open (scale + activate open chest)
		chestSequence.AppendCallback(() =>
		{
			ClosedChest.SetActive(false);
			OpenChest.SetActive(true);
		});
		chestSequence.Append(ChestParent.DOScale(1f, 0.3f).SetEase(Ease.OutBack));

		yield return chestSequence.WaitForCompletion();

		List<Tween> tweens = new();
		for (int i = 0; i < CardDefinitions.Count; i++)
		{
			CardDefinition cardDefinition = CardDefinitions[i];
			FlippableCard card = Instantiate(
				FlippableCardPrefab,
				PackArea
			);

			Vector3 targetLocalPos = new Vector3(startX + i * CardSpacing, 0f, 0f);

			card.transform.position = chestPos;
			card.transform.localRotation = Quaternion.identity;
			card.transform.localScale = Vector3.one;
			card.FlipComplete = FlipComplete;
			card.Setup(cardDefinition);
			card.CanFlip = false;

			float jumpPower = 2f;   // height of jump
			int numJumps = 1;       // number of bounces
			float duration = 0.6f;  // duration of animation
			
			DOVirtual.DelayedCall(i * cascadeDelay, () => {
				Common.Instance.AudioManager.PlayUISound(card.JumpCard);
			});

			var jumpTween = card.transform.DOLocalJump(targetLocalPos, jumpPower, numJumps, duration)
				.SetEase(Ease.OutBack)
				.SetDelay(i * cascadeDelay)
				.OnComplete(() =>
				{
					card.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0.5f);
					card.CanFlip = true;
				});
			tweens.Add(jumpTween);
		}

		foreach (var tween in tweens)
		{
			yield return tween.WaitForCompletion();
		}
		ClosedChest.GetComponent<Button>().interactable = true;
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
			Common.Instance.SaveManager.SaveData.GameSaveData.CardCollection.Add(card.ID);
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
		string returnScreenName = OpenPackScene.ReturnScreenName;
		OpenPackScene.ReturnScreenName = null;
		Common.Instance.SceneTransition.TransitionToOrMain(returnScreenName);
	}
}
