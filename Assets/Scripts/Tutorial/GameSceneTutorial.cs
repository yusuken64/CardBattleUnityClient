using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneTutorial : MonoBehaviour
{
	public Canvas TutorialCanvas;
	public Image TutorialCanvasBG;
	public List<TutorialPopup> Tutorials;
	private GameManager _gameManager;

	private void Start()
	{
		HideTutorials();

		HookIntoGame();
	}

	private void HideTutorials()
	{
		TutorialCanvas.gameObject.SetActive(false);
		foreach (var tutorial in Tutorials)
		{
			tutorial.gameObject.SetActive(false);
		}
	}

	public void HookIntoGame()
	{
		_gameManager = FindFirstObjectByType<GameManager>();
		_gameManager._engine.ActionPlaybackCallback += ActionPlaybackCallback;

		GameManager.GameStartParams.BlockStart = false;
	}

	private void ActionPlaybackCallback(GameState state, (IGameAction action, ActionContext context) current)
	{
		if (state.CurrentPlayer.Id == _gameManager.Player.Data.Id)
		{
			if (state.turn == 1 &&
				current.action is StartTurnAction)
			{
				ShowTutorialMessage("Basic1");
			}
			else if (state.turn == 3 &&
				current.action is StartTurnAction)
			{
				ShowTutorialMessage("Basic2");
			}
			else if (state.turn == 5 &&
				current.action is StartTurnAction)
			{
				ShowTutorialMessage("Basic3");
			}
			else if (state.turn == 7 &&
				current.action is StartTurnAction)
			{
				ShowTutorialMessage("Basic4");
			}
		}
	}

	public void ShowTutorialMessage(string tutorialName)
	{
		var tutorial = Tutorials.FirstOrDefault(x => x.TutorialName == tutorialName);
		if (tutorial == null)
		{
			Debug.LogError($"Tutorial {tutorialName} not found");
			return;
		}

		TutorialCanvas.gameObject.SetActive(true);
		tutorial.Action = TutorialFinished;
		tutorial.Show();

		tutorial.transform.localScale = Vector3.zero;
		TutorialCanvasBG.color = new Color(0, 0, 0, 0);

		DOTween.Sequence()
			.Append(TutorialCanvasBG.DOFade(1f, 0.4f))
			.Join(tutorial.transform
			.DOScale(Vector3.one, 0.5f)
			.SetEase(Ease.OutBack)
			.SetDelay(0.05f)
	);
	}

	public void TutorialFinished(string tutorialName)
	{
		HideTutorials();
		TutorialCanvas.gameObject.SetActive(false);
		Common.Instance.SaveManager.SaveData.TutorialSaveData
			.CompletedTutorials.Add(tutorialName);
	}
}
