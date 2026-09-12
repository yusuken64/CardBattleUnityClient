using System;
using System.Threading;
using System.Threading.Tasks;
//using GameServer.Contracts;
//using Microsoft.AspNetCore.SignalR.Client;
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
    public TextMeshProUGUI NetworkMessage;

    //private HubConnection _connection;
    private CancellationTokenSource _cts;

    void Awake()
    {
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
        NetworkLoadingOverlay.SetActive(false);
    }

    private void Close()
    {
        _cts?.Cancel();
        gameObject.SetActive(false);
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
        string matchIdText = MatchIdInput.text.Trim();
        if (!Guid.TryParse(matchIdText, out var matchGuid))
        {
            JoinErrorText.text = "Invalid match id.";
            JoinErrorText.gameObject.SetActive(true);
            return;
        }

        StartNetworkedGame(NetworkJoinMode.Join, matchGuid);
    }

    private void StartNetworkedGame(NetworkJoinMode joinMode, Guid? matchId)
    {
        var activeDeck = Common.Instance.SaveManager.SaveData.GameSaveData.GetActiveDeck();
        GameManager.GameStartParams = new GameStartParams
        {
            CombatDeck = activeDeck?.ToDeck()
        };
        GameManager.PendingStartArgs = new StartGameArgs
        {
            Mode = GameMode.Networked,
            JoinMode = joinMode,
            MatchId = matchId?.ToString()
        };

        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
    }

    // Shared connect + run + cleanup wrapper. `flow` does the actual JoinQueue/CreateMatch/JoinMatch
    // call and returns the resolved matchId (or throws on failure).
    private async Task RunFlow(Func<Task<Guid>> flow, string waitingMessage)
    {
        SetBusy(true, waitingMessage);
        _cts = new CancellationTokenSource();

        try
        {
            //_connection = new HubConnectionBuilder()
            //    .WithUrl($"{GameConfig.ServerUrl}/hubs/match")
            //    .Build();

            //await _connection.StartAsync(_cts.Token);

            //var matchId = await flow();

            //GameManager.PendingStartArgs = new StartGameArgs
            //{
            //    Mode = GameMode.Networked,
            //    MatchId = matchId.ToString(),
            //    Connection = _connection, // hand the live connection off to GameScene
            //};

            Common.Instance.SceneTransition.DoTransition(() =>
            {
                SceneManager.LoadScene("GameScene");
            });
            // Deliberately not calling SetBusy(false)/disposing _connection here — the scene
            // transition takes over and GameScene owns the connection from this point on.
        }
        catch (OperationCanceledException)
        {
            await DisposeConnection();
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
            await DisposeConnection();
        }
        finally
        {
            if (this != null && gameObject.activeSelf)
                SetBusy(false, null);
        }
    }

    private async Task<Guid> QuickMatchFlow()
    {
        var matchFound = new TaskCompletionSource<Guid>();
        //_connection.On<Guid>("OnMatchFound", id => matchFound.TrySetResult(id));

        var activeDeck = Common.Instance.SaveManager.SaveData.GameSaveData.GetActiveDeck();
        //await _connection.InvokeAsync("JoinQueue", activeDeck, _cts.Token);

        using (_cts.Token.Register(() => matchFound.TrySetCanceled()))
        {
            return await matchFound.Task;
        }
    }

    //private async Task<Guid> HostFlow()
    //{
    //    var activeDeck = Common.Instance.SaveManager.SaveData.GameSaveData.GetActiveDeck();
    //    return await _connection.InvokeAsync<Guid>("CreateMatch", activeDeck, _cts.Token);
    //}

    private async Task<Guid> JoinFlow(Guid matchId)
    {
        var activeDeck = Common.Instance.SaveManager.SaveData.GameSaveData.GetActiveDeck();
        //var result = await _connection.InvokeAsync<JoinResult>("JoinMatch", matchId, activeDeck, _cts.Token);
        //if (!result.Success)
        //{
        //    throw new InvalidOperationException(result.Error);
        //}
        return matchId;
    }

    private async Task DisposeConnection()
    {
        //if (_connection != null)
        //{
        //    await _connection.StopAsync();
        //    await _connection.DisposeAsync();
        //    _connection = null;
        //}
    }

    private void SetBusy(bool busy, string message)
    {
        NetworkLoadingOverlay.SetActive(busy);
        if (busy) NetworkMessage.text = message;

        QuickMatchButton.interactable = !busy;
        HostButton.interactable = !busy;
        JoinButton.interactable = !busy;
        JoinConfirmButton.interactable = !busy;
        JoinBackButton.interactable = !busy;
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
        _cts?.Cancel();
    }
}
