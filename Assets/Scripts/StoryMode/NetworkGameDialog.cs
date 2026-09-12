using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public GameObject NetworkLoadingOverlay;
    public TMP_InputField NetworkMessage;
    public Button CancelWaitingButton;
    public Button CopyMatchCodeButton;

    public string ServerUrl = "http://localhost:5299";

    private NetworkGameSession _session;
    private bool _transitioning;
    private CancellationTokenSource _cts;
    private string _hostedMatchCode;

    void Awake()
    {
        QuickMatchButton.onClick.AddListener(QuickMatch_Click);
        HostButton.onClick.AddListener(Host_Click);
        JoinButton.onClick.AddListener(Join_Click);
        CloseButton.onClick.AddListener(Close);

        JoinConfirmButton.onClick.AddListener(JoinConfirm_Click);
        JoinBackButton.onClick.AddListener(ShowPicker);

        CancelWaitingButton.onClick.AddListener(CancelWaiting);
        CancelWaitingButton.gameObject.SetActive(false);
        CopyMatchCodeButton.onClick.AddListener(CopyMatchCode);
        SetCopyCode(null);
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
        SetBusy(_cts != null, "Connecting to server...");
    }

    private void Close()
    {
        if (_transitioning) return;
        _cts?.Cancel();
        gameObject.SetActive(false);
    }

    private void CancelWaiting()
    {
        if (_transitioning || _cts == null || _cts.IsCancellationRequested) return;
        CancelWaitingButton.interactable = false;
        SetCopyCode(null);
        NetworkMessage.text = "Cancelling...";
        _cts.Cancel();
        if (_session != null)
        {
            if (Common.Instance != null) _ = Common.Instance.EndNetworkSessionAsync(_session);
            else _ = _session.StopAsync();
        }
    }

    private void QuickMatch_Click() =>
        StartNetworkedGame(NetworkJoinMode.QuickMatch, null);

    private void Host_Click() =>
        StartNetworkedGame(NetworkJoinMode.Host, null);

    private void Join_Click()
    {
        MatchIdInput.text = "";
        JoinErrorText.gameObject.SetActive(false);
        PickerPanel.SetActive(false);
        JoinPanel.SetActive(true);
        MatchIdInput.Select();
        MatchIdInput.ActivateInputField();
    }

    private void JoinConfirm_Click()
    {
        string joinCode = System.Text.RegularExpressions.Regex.Replace(MatchIdInput.text, @"[\s-]", "").ToUpperInvariant();
        if (!System.Text.RegularExpressions.Regex.IsMatch(joinCode, "^[A-HJ-NP-Z2-9]{6}$"))
        {
            JoinErrorText.text = "Enter a six-character join code, such as K7MP-4X.";
            JoinErrorText.gameObject.SetActive(true);
            return;
        }

        StartNetworkedGame(NetworkJoinMode.Join, joinCode);
    }

    private async void StartNetworkedGame(NetworkJoinMode joinMode, string joinCode)
    {
        if (_cts != null || _transitioning) return;

        var cts = new CancellationTokenSource();
        _cts = cts;
        NetworkGameSession session = null;
        bool enteredGame = false;
        var common = Common.Instance;
        SetBusy(true, "Connecting to server...");
        foreach (var label in CancelWaitingButton.GetComponentsInChildren<TMP_Text>(true))
            label.text = joinMode == NetworkJoinMode.Host ? "Stop Hosting" :
                joinMode == NetworkJoinMode.QuickMatch ? "Cancel Search" : "Cancel";
        JoinErrorText.gameObject.SetActive(false);

        try
        {
            var activeDeck = Common.Instance.SaveManager.SaveData.GameSaveData.GetActiveDeck();
            if (activeDeck == null)
                throw new InvalidOperationException("Select a deck before starting a network game.");

            var decklist = activeDeck.ToDeck().ToDecklistRequest(
                joinMode == NetworkJoinMode.Join ? "Joiner" : "Host");
            session = common.CreateNetworkSession(ServerUrl);
            _session = session;
            await session.ConnectAsync();
            cts.Token.ThrowIfCancellationRequested();

            switch (joinMode)
            {
                case NetworkJoinMode.QuickMatch:
                    NetworkMessage.text = "Searching for an opponent...";
                    await session.Client.InvokeAsync<object>("JoinQueue", decklist);
                    break;
                case NetworkJoinMode.Host:
                    session.MatchId = await session.Client.InvokeAsync<string>("CreateMatch", decklist);
                    cts.Token.ThrowIfCancellationRequested();
                    if (!IsValidMatchCode(session.MatchId))
                        throw new InvalidOperationException("The server returned an unsupported match ID. Restart the server with the updated short-code build.");
                    NetworkMessage.text = $"Join code: {session.MatchId}\nWaiting for an opponent...";
                    SetCopyCode(session.MatchId);
                    break;
                case NetworkJoinMode.Join:
                    var result = await session.Client.InvokeAsync<JoinResult>("JoinMatch", joinCode, decklist);
                    cts.Token.ThrowIfCancellationRequested();
                    if (result == null || !result.Success)
                        throw new InvalidOperationException(result?.Error ?? "Unable to join this match.");
                    if (!IsValidMatchCode(result.MatchId))
                        throw new InvalidOperationException("The server returned an invalid match.");
                    session.MatchId = result.MatchId;
                    NetworkMessage.text = "Waiting for the game to start...";
                    break;
            }

            // A queue acknowledgement or hosted match id does not mean the game has started.
            while (true)
            {
                cts.Token.ThrowIfCancellationRequested();
                if (session.Disconnected || !session.Client.IsConnected)
                    throw new InvalidOperationException("Disconnected from the server before the game started.");
                if (session.HasEnded || session.LatestView?.IsGameOver == true)
                    throw new InvalidOperationException("The match ended before the game could start.");
                if (session.IsReady && !Common.Instance.SceneTransition.transitionInProgress) break;
                await Task.Delay(50, cts.Token);
            }

            _transitioning = true;
            CloseButton.interactable = false;
            CancelWaitingButton.interactable = false;
            CopyMatchCodeButton.interactable = false;
            var transitionComplete = new TaskCompletionSource<bool>();
            Common.Instance.SceneTransition.DoTransition(() =>
            {
                // Recheck after the fade: cancellation or server failure must not load GameScene.
                if (cts.IsCancellationRequested || !session.IsReady)
                {
                    transitionComplete.TrySetResult(false);
                    return;
                }
                GameManager.GameStartParams = null;
                GameManager.PendingStartArgs = new StartGameArgs
                {
                    Mode = GameMode.Networked,
                    JoinMode = joinMode,
                    MatchId = session.MatchId
                };
                enteredGame = true;
                _session = null;
                SceneManager.LoadScene("GameScene");
                transitionComplete.TrySetResult(true);
            });
            if (!await transitionComplete.Task)
            {
                cts.Token.ThrowIfCancellationRequested();
                throw new InvalidOperationException("The server game is no longer available.");
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            if (!cts.IsCancellationRequested && this != null && gameObject.activeSelf)
                ShowError(ex.Message);
        }
        finally
        {
            if (!enteredGame && session != null)
            {
                if (common != null) await common.EndNetworkSessionAsync(session);
                else await session.StopAsync();
            }
            _session = null;
            _cts = null;
            bool cancelled = cts.IsCancellationRequested;
            cts.Dispose();
            if (this != null && !enteredGame)
            {
                _transitioning = false;
                CloseButton.interactable = true;
                SetBusy(false, null);
                if (cancelled && gameObject.activeSelf) ShowPicker();
            }
        }
    }

    private static bool IsValidMatchCode(string matchId) =>
        matchId != null && System.Text.RegularExpressions.Regex.IsMatch(matchId, "^[A-HJ-NP-Z2-9]{4}-[A-HJ-NP-Z2-9]{2}$");

    private void SetBusy(bool busy, string message)
    {
        SetCopyCode(null);
        NetworkLoadingOverlay.SetActive(busy);
        CancelWaitingButton.gameObject.SetActive(busy);
        CancelWaitingButton.interactable = busy && !_transitioning && _cts?.IsCancellationRequested != true;
        if (busy) NetworkMessage.text = message;

        QuickMatchButton.interactable = !busy;
        HostButton.interactable = !busy;
        JoinButton.interactable = !busy;
        JoinConfirmButton.interactable = !busy;
        JoinBackButton.interactable = !busy;
    }

    private void SetCopyCode(string code)
    {
        _hostedMatchCode = code;
        CopyMatchCodeButton.gameObject.SetActive(!string.IsNullOrEmpty(code));
        CopyMatchCodeButton.interactable = !string.IsNullOrEmpty(code);
        foreach (var label in CopyMatchCodeButton.GetComponentsInChildren<TMP_Text>(true))
            label.text = "Copy Code";
    }

    private void CopyMatchCode()
    {
        if (string.IsNullOrEmpty(_hostedMatchCode) || _transitioning || _cts?.IsCancellationRequested != false)
            return;
        GUIUtility.systemCopyBuffer = _hostedMatchCode;
        foreach (var label in CopyMatchCodeButton.GetComponentsInChildren<TMP_Text>(true))
            label.text = "Copied!";
    }

    private void ShowError(string message)
    {
        PickerPanel.SetActive(false);
        JoinPanel.SetActive(true);
        JoinErrorText.text = message;
        JoinErrorText.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // Loading GameScene keeps Common's session alive; closing matchmaking ends it.
        if (_session == null) return;
        _cts?.Cancel();
        if (Common.Instance != null) _ = Common.Instance.EndNetworkSessionAsync(_session);
        else _ = _session.StopAsync();
    }
}
