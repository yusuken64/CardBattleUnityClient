using System.Collections;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
	public GameObject TurnStartObject;
	public EndTurnButton EndTurnButton;

	public TextMeshProUGUI Message;
    private Coroutine messageCoroutine;

	private void Start()
	{
        Message.gameObject.SetActive(false);
	}

	public void ShowMessage(string message)
    {
        // Stop any previous message
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        // Start new message coroutine
        messageCoroutine = StartCoroutine(ShowMessageCoroutine(message));
    }

    private IEnumerator ShowMessageCoroutine(string message)
    {
        Message.text = message;
        Message.gameObject.SetActive(true);

        // Wait for 3 seconds
        yield return new WaitForSeconds(3f);

        Message.gameObject.SetActive(false);
        messageCoroutine = null;
    }
}
