using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

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
		DoTransition(action != null ? () => WrapAction(action) : null);
	}

	public void DoTransition(Func<IEnumerator> action)
	{
		if (transitionInProgress)
		{
			return;
		}

		gameObject.SetActive(true);
		HintText.text = Hints[UnityEngine.Random.Range(0, Hints.Count)];
		StartCoroutine(DoTransitionRoutine(action));
	}

	private IEnumerator DoTransitionRoutine(Func<IEnumerator> action)
	{
		transitionInProgress = true;

		Curtains.alpha = 0;

		yield return Curtains
			.DOFade(1f, TransitionTimeSeconds)
			.SetUpdate(true)
			.WaitForCompletion();

		if (action != null)
		{
			yield return action();
		}

		yield return null;

		yield return Curtains
			.DOFade(0f, TransitionTimeSeconds)
			.SetUpdate(true)
			.WaitForCompletion();

		transitionInProgress = false;
		gameObject.SetActive(false);
	}

	private IEnumerator WrapAction(Action action)
	{
		action.Invoke();
		yield break;
	}

	public void TransitionToOrMain(string returnScreenName)
	{
		string targetScene = string.IsNullOrWhiteSpace(returnScreenName)
			? "Main"
			: returnScreenName;

		DoTransition(() =>
		{
			SceneManager.LoadScene(targetScene);
		});
	}
}
