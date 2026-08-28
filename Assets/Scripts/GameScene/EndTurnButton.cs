using CardBattleEngine;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndTurnButton : MonoBehaviour
{
	public TextMeshProUGUI ButtonText;
	public Image ButtonImage;
	public Button Button;
	public GameObject OnlyActionIndicator;
	public GameObject OnlyActionIndicator2;

	public AudioClip NoActionSound;

	public Color ReadyColor;
	public Color OnlyColor;
	public Color EnemyColor;
	private GameManager _gameManager;
	private UI _ui;

	private void Start()
	{
		_gameManager = FindFirstObjectByType<GameManager>();
		_ui = FindFirstObjectByType<UI>();
		OnlyActionIndicator.gameObject.SetActive(false);
		OnlyActionIndicator2.gameObject.SetActive(false);
	}

	public void SetToReady()
	{
		ButtonImage.color = ReadyColor;
		this.Button.interactable = true;
		ButtonText.text = "End Turn";
		OnlyActionIndicator.gameObject.SetActive(false);
		OnlyActionIndicator2.gameObject.SetActive(false);
	}

	public void SetToOnlyAction()
	{
		ButtonText.text = "End Turn";
		OnlyActionIndicator.gameObject.SetActive(true);
		OnlyActionIndicator2.gameObject.SetActive(true);
		Common.Instance.AudioManager.PlayUISound(NoActionSound);
	}

	public void SetToEnemyTurn()
	{
		ButtonImage.color = EnemyColor;
		ButtonText.text = "Enemy Turn";
		OnlyActionIndicator.gameObject.SetActive(false);
		OnlyActionIndicator2.gameObject.SetActive(false);
	}

	public void OnClick()
	{
		if (_gameManager._gameState.CurrentPlayer.Name == _gameManager.Player.Data.Name)
		{
			SetToUnclickable();
			_gameManager.ActivePlayerTurn = false;
			_gameManager.ResolveAction(
				new EndTurnAction(),
				new ActionContext()
				{
					Source = _gameManager.Player.Data,
					SourcePlayer = _gameManager.Player.Data
				}
				);
		}
		else
		{
			_ui.WarnEnemyTurn();
		}
	}

	private void SetToUnclickable()
	{
		SetToEnemyTurn();
		this.Button.interactable = false;
	}
}
