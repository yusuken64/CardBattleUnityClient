using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
	private PointerInput _pointerInput;

	public void BackGround_Clicked()
	{
		CloseSettingsScreen();
	}

	public void Settings_Clicked()
	{

	}

	public void Forfeit_Clicked()
	{
		CloseSettingsScreen();

		var gameManager =FindFirstObjectByType<GameManager>();

		gameManager.ResolveAction(
			new CardBattleEngine.DeathAction(),
			new CardBattleEngine.ActionContext()
			{
				SourcePlayer = gameManager.Player.Data,
				Target = gameManager.Player.Data
			});
	}

	internal void Open()
	{
		_pointerInput = FindFirstObjectByType<PointerInput>();
		_pointerInput.gameObject.SetActive(false);
	}

	public void CloseSettingsScreen()
	{
		this.gameObject.SetActive(false);
		_pointerInput.gameObject.SetActive(true);
	}
}
