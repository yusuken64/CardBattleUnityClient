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
	public GameObject BlackCurtains;
	public TextMeshProUGUI HintText;
	public List<string> Hints;

	public GameObject DungeonDoor;
	public Vector3 DungeonDoorStartPos;
	public Vector3 DungeonDoorMidPos;
	public Vector3 DungeonDoorEndPos;
	public float DoorTime = 1f;
	public float DoorDelayTime = 1f;

	internal bool transitionInProgress;

	private void Start()
	{
		this.gameObject.SetActive(false);
		DungeonDoor.gameObject.SetActive(false);
	}

	public void DoTransition(Action action)
	{
		DoTransition(action != null ? () => WrapAction(action) : null);
	}

	[ContextMenu("TestDoor")]
	public void DoDoorTransitionTest()
	{
		DoDoorTransition(null);
	}

	public void DoDoorTransition(Action action)
	{
		if (transitionInProgress)
		{
			return;
		}
		gameObject.SetActive(true);
		StartCoroutine(DoDoorTransitionRoutine(action != null ? () => WrapAction(action) : null));
	}

	private IEnumerator DoDoorTransitionRoutine(Func<IEnumerator> action)
	{
		transitionInProgress = true;
		Curtains.alpha = 1;

		DungeonDoor.gameObject.SetActive(true);
		BlackCurtains.gameObject.SetActive(false);

		DungeonDoor.gameObject.SetActive(true);
		DungeonDoor.gameObject.transform.localPosition = DungeonDoorStartPos;
		var closeTween = DungeonDoor.gameObject.transform.DOLocalMove(DungeonDoorMidPos, DoorTime)
			.SetEase(Ease.OutBounce);

		yield return closeTween.WaitForCompletion();

		if (action != null)
		{
			yield return action();
		}

		yield return new WaitForSeconds(DoorDelayTime);

		var openTween = DungeonDoor.gameObject.transform.DOLocalMove(DungeonDoorEndPos, DoorTime);

		yield return openTween.WaitForCompletion();

		transitionInProgress = false;
		gameObject.SetActive(false);
		DungeonDoor.gameObject.SetActive(false);
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
		DungeonDoor.gameObject.SetActive(false);
		BlackCurtains.gameObject.SetActive(true);

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
