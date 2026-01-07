using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
	public float TransitionTimeSeconds;
	public CanvasGroup Curtains;

	public TextMeshProUGUI HintText;

	public List<string> Hints;

	private bool transitionInProgress;

	private void Start()
	{
		this.gameObject.SetActive(false);
	}

	public void DoTransition(Action action)
	{
		if (transitionInProgress)
			return;

		gameObject.SetActive(true);
		HintText.text = Hints[UnityEngine.Random.Range(0, Hints.Count)];
		StartCoroutine(DoTransitionRoutine(action));
	}

	private IEnumerator DoTransitionRoutine(Action action)
	{
		transitionInProgress = true;

		Curtains.alpha = 0;

		yield return Curtains
			.DOFade(1f, TransitionTimeSeconds)
			.SetUpdate(true)
			.WaitForCompletion();

		action?.Invoke();

		yield return null;

		yield return Curtains
			.DOFade(0f, TransitionTimeSeconds)
			.SetUpdate(true)
			.WaitForCompletion();

		transitionInProgress = false;
		gameObject.SetActive(false);
	}
}
