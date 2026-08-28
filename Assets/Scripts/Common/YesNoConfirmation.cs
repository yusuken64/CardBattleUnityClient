using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class YesNoConfirmation : MonoBehaviour
{
	public TextMeshProUGUI PromptTitle;
	public TextMeshProUGUI Prompt;

	public TextMeshProUGUI YesText;
	public Button YesButton;
	public TextMeshProUGUI NoText;
	public Button NoButton;

    private Action _yesAction;
    private Action _noAction;

    void Awake()
    {
        // Wire buttons once
        YesButton.onClick.AddListener(OnYesClicked);
        NoButton.onClick.AddListener(OnNoClicked);
    }

    public void Setup(
        string promptTitle,
        string prompt,
        string yesText,
        Action yesAction,
        string noText,
        Action noAction)
    {
        PromptTitle.text = promptTitle;
        Prompt.text = prompt;

        YesText.text = yesText;
        NoText.text = noText;

        _yesAction = yesAction;
        _noAction = noAction;

        gameObject.SetActive(true);

        YesButton.Select();
    }

    private void OnYesClicked()
    {
        _yesAction?.Invoke();
        Close();
    }

    private void OnNoClicked()
    {
        _noAction?.Invoke();
        Close();
    }

    private void Close()
    {
        gameObject.SetActive(false);

        // Prevent accidental reuse bugs
        _yesAction = null;
        _noAction = null;
    }
}
