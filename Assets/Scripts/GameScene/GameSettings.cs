using System;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
	private PointerInput _pointerInput;

	public void BackGround_Clicked()
	{
		CloseSettingsScreen();
	}

	public void GlobalSettings_Clicked()
	{
		Common.Instance.GlobalSettings.gameObject.SetActive(true);
		Common.Instance.GlobalSettings.SetToAudioSettings();
	}

	public void Forfeit_Clicked()
	{
		CloseSettingsScreen();

		var gameManager =FindFirstObjectByType<GameManager>();

		gameManager.Forfeit();
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
