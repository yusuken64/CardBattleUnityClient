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
		ButtonText.text = "End Turn";
	}

	public void SetToOnlyAction()
	{
		ButtonText.text = "End Turn!";
	}

	public void SetToEnemyTurn()
	{
		ButtonText.text = "Enemy Turn";
	}

	public void OnClick()
	{
		ValidateState();
		if (_gameManager._gameState.CurrentPlayer.Name == _gameManager.Player.Data.Name)
		{
			_gameManager.ResolveAction(
				new EndTurnAction(),
				new ActionContext() { SourcePlayer = _gameManager.Player.Data }
				);
		}
		else
		{
			_ui.ShowMessage("Not your turn");
		}
	}

	public void ValidateState()
	{
		var gameManager = FindFirstObjectByType<GameManager>();

		var state = gameManager._gameState;
		PlayersEqual(gameManager.Player, state.Players[0]);
		//PlayersEqual(gameManager.Opponent, state.Players[1]);
	}

	private void PlayersEqual(Player player, CardBattleEngine.Player data)
	{
		for (int i = 0; i < data.Board.Count; i++)
		{
			CardBattleEngine.Minion minionData = data.Board[i];
			var boardMinion = player.Board.Minions[i];

			AssertAreEqual(boardMinion.Data, minionData);
			AssertAreEqual(boardMinion.Attack, minionData.Attack);
			AssertAreEqual(boardMinion.Health, minionData.Health);
			AssertAreEqual(boardMinion.CanAttack, minionData.CanAttack());
		}
	}

	private void AssertAreEqual<T>(T a, T b)
	{
		if (!EqualityComparer<T>.Default.Equals(a, b))
		{
			throw new Exception($"Validation exception: {a} != {b}");
		}
	}
}
