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

	public Color ReadyColor;
	public Color OnlyColor;
	public Color EnemyColor;
	private GameManager _gameManager;
	private UI _ui;

	private void Start()
	{
		_gameManager = FindFirstObjectByType<GameManager>();
		_ui = FindFirstObjectByType<UI>();
	}

	public void SetToReady()
	{
		ButtonImage.color = ReadyColor;
		this.Button.interactable = true;
		ButtonText.text = "End Turn";
	}

	public void SetToOnlyAction()
	{
		ButtonText.text = "End Turn!";
	}

	public void SetToEnemyTurn()
	{
		ButtonImage.color = EnemyColor;
		ButtonText.text = "Enemy Turn";
	}

	public void OnClick()
	{
		if (_gameManager._gameState.CurrentPlayer.Name == _gameManager.Player.Data.Name)
		{
			SetToUnclickable();
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
			_ui.ShowMessage("Not your turn");
		}
	}

	private void SetToUnclickable()
	{
		SetToEnemyTurn();
		this.Button.interactable = false;
	}
}
