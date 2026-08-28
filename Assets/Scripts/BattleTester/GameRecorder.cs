using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System.Threading.Tasks;

public class GameRecorder : MonoBehaviour
{
	public string Url = "https://localhost:44351/record";

    [ContextMenu("Write Test Record")]
    public void WriteTestRecord()
    {
        // Fire and forget
        _ = SendRecordAsync(new GameRecordDto
        {
            GameId = "Test",
            CardID = "Test",
            DidWin = Random.value > 0.5f,
            PlayerOrder = (byte)(Random.value > 0.5f ? 1 : 2),
            WasDrawn = Random.value > 0.5f,
            WasPlayed = true,
            CopiesPlayed = Random.Range(1, 3),
            TurnPlayed = Random.Range(1, 10),
            TotalTurns = 10
        });
    }

    public async Task SendRecordAsync(GameRecordDto record)
    {
        string json = JsonUtility.ToJson(record);

        using UnityWebRequest request = new UnityWebRequest(Url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        var operation = request.SendWebRequest();
        while (!operation.isDone)
            await Task.Yield();

#if UNITY_2020_1_OR_NEWER
        if (request.result != UnityWebRequest.Result.Success)
#else
        if (request.isNetworkError || request.isHttpError)
#endif
        {
            Debug.LogError("Failed to send record: " + request.error);
        }
        else
        {
            Debug.Log("Test record sent successfully!");
        }
    }
}

[System.Serializable]
public class GameRecordDto
{
    public string GameId;
    public string CardID;
    public bool DidWin;
    public byte PlayerOrder;
    public bool? WasDrawn;
    public bool WasPlayed;
    public int CopiesPlayed;
    public int TurnPlayed;
    public int TotalTurns;
}