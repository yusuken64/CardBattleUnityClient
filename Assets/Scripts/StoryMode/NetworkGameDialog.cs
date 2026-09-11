using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Dialog opened from StoryModeScene's networked game button. Offers the same three choices as
// CardBattleEngine.GamePlayer's RemoteGameClient.CreateOrJoinMatch (quick match / host / join by id),
// then hands StartGameArgs to GameManager and loads GameScene.
public class NetworkGameDialog : MonoBehaviour
{
	public GameObject PickerPanel;
	public GameObject JoinPanel;

	public Button QuickMatchButton;
	public Button HostButton;
	public Button JoinButton;
	public Button CloseButton;

	public TMP_InputField MatchIdInput;
	public TextMeshProUGUI JoinErrorText;
	public Button JoinConfirmButton;
	public Button JoinBackButton;

	void Awake()
	{
		// Wire buttons once
		QuickMatchButton.onClick.AddListener(QuickMatch_Click);
		HostButton.onClick.AddListener(Host_Click);
		JoinButton.onClick.AddListener(Join_Click);
		CloseButton.onClick.AddListener(Close);

		JoinConfirmButton.onClick.AddListener(JoinConfirm_Click);
		JoinBackButton.onClick.AddListener(ShowPicker);
	}

	public void Show()
	{
		gameObject.SetActive(true);
		ShowPicker();
	}

	private void ShowPicker()
	{
		PickerPanel.SetActive(true);
		JoinPanel.SetActive(false);
	}

	private void Close()
	{
		gameObject.SetActive(false);
	}

	private void QuickMatch_Click()
	{
		StartNetworkedGame(new StartGameArgs
		{
			Mode = GameMode.Networked,
			JoinMode = NetworkJoinMode.QuickMatch
		});
	}

	private void Host_Click()
	{
		StartNetworkedGame(new StartGameArgs
		{
			Mode = GameMode.Networked,
			JoinMode = NetworkJoinMode.Host
		});
	}

	private void Join_Click()
	{
		MatchIdInput.text = "";
		JoinErrorText.gameObject.SetActive(false);
		PickerPanel.SetActive(false);
		JoinPanel.SetActive(true);
		MatchIdInput.Select();
	}

	private void JoinConfirm_Click()
	{
		string matchIdText = MatchIdInput.text;
		if (!Guid.TryParse(matchIdText, out _))
		{
			JoinErrorText.text = "Invalid match id.";
			JoinErrorText.gameObject.SetActive(true);
			return;
		}

		StartNetworkedGame(new StartGameArgs
		{
			Mode = GameMode.Networked,
			JoinMode = NetworkJoinMode.Join,
			MatchId = matchIdText
		});
	}

	private void StartNetworkedGame(StartGameArgs args)
	{
		GameManager.PendingStartArgs = args;
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			SceneManager.LoadScene("GameScene");
		});
	}
}
