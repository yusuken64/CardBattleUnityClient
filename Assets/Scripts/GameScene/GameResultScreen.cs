using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultScreen : MonoBehaviour
{
	public Image HeroImage;
	public GameObject WinObject;
	public GameObject LoseObject;

	public GameObject OkButton;

	public void Ok_Clicked()
	{
		SceneManager.LoadScene("Main");
	}

	[ContextMenu("Test Win")]
	public void TestAnimation_Win()
	{
		this.gameObject.SetActive(true);
		StartCoroutine(DoGameEndRoutine(true));
	}

	[ContextMenu("Test Lose")]
	public void TestAnimation_Lose()
	{
		this.gameObject.SetActive(true);
		StartCoroutine(DoGameEndRoutine(false));
	}

	internal IEnumerator DoGameEndRoutine(bool isWin)
	{
		this.gameObject.SetActive(true);

		var gameManager = FindFirstObjectByType<GameManager>();
		HeroImage.sprite = gameManager.Player.HeroImage.sprite;

		var pointerInput = FindFirstObjectByType<PointerInput>();
		pointerInput.gameObject.SetActive(false);

		// Ensure everything starts hidden
		WinObject.SetActive(false);
		LoseObject.SetActive(false);
		OkButton.SetActive(false);

		// Fade in hero image
		HeroImage.color = new Color(1, 1, 1, 0);
		HeroImage.DOFade(1f, 0.5f);

		// Play "win" or "lose" headline
		var headline = isWin ? WinObject : LoseObject;
		headline.SetActive(true);

		// Main celebration sequence
		Sequence seq = DOTween.Sequence();

		seq.Append(
			HeroImage.transform
				.DOScale(0.410736f, 0.4f)
				.SetEase(Ease.OutBack)
		);

		seq.Join(
			HeroImage.transform
				.DORotate(new Vector3(0, 0, 5f), 0.4f)
				.SetLoops(2, LoopType.Yoyo)
				.SetEase(Ease.InOutSine)
		);

		seq.Append(
			headline.transform
				.DOPunchScale(new Vector3(0.3f, 0.3f, 0), 0.6f, 8)
		);

		seq.AppendInterval(0.3f);

		// Reveal the OK button with a bounce
		seq.AppendCallback(() =>
		{
			OkButton.SetActive(true);
			OkButton.transform.localScale = Vector3.zero;
			OkButton.transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack);
		});

		// Wait for the sequence to finish
		yield return seq.WaitForCompletion();
	}
}
