using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NetworkLobby : MonoBehaviour
{
	public InputField ServerUrlInput;
	public static string OverrideServerUrl;

	/// <summary>
	/// Called when Create Match button is clicked.
	/// Sets up a networked game in create/host mode and loads the GameScene.
	/// </summary>
	public void OnCreateMatchClicked()
	{
		UpdateServerUrlIfNeeded();
		GameManager.PendingStartArgs = new StartGameArgs { Mode = GameMode.Networked };
		SceneManager.LoadScene("GameScene");
	}

	/// <summary>
	/// Called when Join Match button is clicked.
	/// Parses the match ID string as a GUID. If valid, sets up a networked game in join mode
	/// and loads the GameScene. If invalid, logs an error and does nothing.
	/// </summary>
	/// <param name="matchIdText">The match ID string to parse as a GUID</param>
	public void OnJoinMatchClicked(string matchIdText)
	{
		if (!Guid.TryParse(matchIdText, out Guid matchId))
		{
			Debug.LogError("Invalid match id");
			return;
		}

		UpdateServerUrlIfNeeded();
		GameManager.PendingStartArgs = new StartGameArgs
		{
			Mode = GameMode.Networked,
			MatchId = matchIdText
		};
		SceneManager.LoadScene("GameScene");
	}

	/// <summary>
	/// Called when Quick Match button is clicked.
	/// Sets up a networked game with an empty MatchId (which causes host/create behavior).
	/// Loads the GameScene.
	/// </summary>
	public void OnQuickMatchClicked()
	{
		UpdateServerUrlIfNeeded();
		GameManager.PendingStartArgs = new StartGameArgs
		{
			Mode = GameMode.Networked,
			MatchId = ""
		};
		SceneManager.LoadScene("GameScene");
	}

	/// <summary>
	/// If ServerUrlInput is provided and non-empty, updates OverrideServerUrl.
	/// </summary>
	private void UpdateServerUrlIfNeeded()
	{
		if (ServerUrlInput != null && !string.IsNullOrEmpty(ServerUrlInput.text))
		{
			OverrideServerUrl = ServerUrlInput.text;
		}
	}
}
