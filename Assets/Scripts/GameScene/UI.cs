using CardBattleEngine;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
	public GameObject TurnStartObject;
	public EndTurnButton EndTurnButton;

	public TextMeshProUGUI Message;
    private Coroutine messageCoroutine;

    public DamageNumber DamageNumberPrefab;
    public DamageNumber HealNumberPrefab;
    public Card CardPreview;
    public GameObject HoverCardPreviewObject;
    public Card HoverCardPreview;
    public GameObject TriggeredEffectParticlePrefab;

    public GameResultScreen GameResultScreen;
    public GameSettings GameSettingsScreen;
    public GameObject SettingsButton;

    public Color DefaultTextColor;
    public Color DamagedTextColor;
    public Color BuffedTextColor;

    public AudioClip ErrorSound;

	private void Start()
	{
        Message.gameObject.SetActive(false);
        GameResultScreen.gameObject.SetActive(false);
        GameSettingsScreen.gameObject.SetActive(false);
        CardPreview.gameObject.SetActive(false);
        HoverCardPreviewObject.gameObject.SetActive(false);
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

    public void WarnEnemyTurn()
	{
		ShowWarningMessage("Not your Turn");
	}

    public void ShowWarningMessage(string warning)
    {
        Common.Instance.AudioManager.PlayUISound(ErrorSound);
        ShowMessage(warning);
	}

	internal IEnumerator DoGameEndRoutine(bool isWin)
    {
        this.gameObject.SetActive(true);
        SettingsButton.SetActive(false);
        yield return GameResultScreen.DoGameEndRoutine(isWin);
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

	internal void ShowDamage(int damage, Transform target)
	{
        var damageNumber = Instantiate(DamageNumberPrefab);
        damageNumber.DamageText.text= $"{damage}";
        damageNumber.transform.position = target.transform.position;

        Transform t = damageNumber.transform;

        // starting scale (small pop)
        t.localScale = Vector3.zero;

        // Build the sequence
        var seq = DOTween.Sequence();

        seq.Append(t.DOScale(1f, 0.15f))               // pop in
           .Join(t.DOMoveY(t.position.y + 0.5f, 0.6f)) // float upward
           .Join(damageNumber.Background.DOFade(0f, 0.6f)) // fade out
           .SetEase(Ease.OutQuad)
           .OnComplete(() =>
           {
               Destroy(damageNumber.gameObject);
           });
    }

    public void ShowHeal(int heal, Transform target)
    {
        var healNumber = Instantiate(HealNumberPrefab);
        healNumber.DamageText.text = $"{heal}";
        healNumber.transform.position = target.transform.position;

        Transform t = healNumber.transform;

        // starting scale (small pop)
        t.localScale = Vector3.zero;

        // Build the sequence
        var seq = DOTween.Sequence();

        seq.Append(t.DOScale(1f, 0.15f))               // pop in
           .Join(t.DOMoveY(t.position.y + 0.5f, 0.6f)) // float upward
           .Join(healNumber.Background.DOFade(0f, 0.6f)) // fade out
           .SetEase(Ease.OutQuad)
           .OnComplete(() =>
           {
               Destroy(healNumber.gameObject);
           });
    }

    internal Color GetColor(int current, int baseValue, int maxValue)
    {
        if (current < maxValue)
        {
            return DamagedTextColor;
        }

        if (current > baseValue)
        {
            return BuffedTextColor;
        }

        return DefaultTextColor;
    }

    internal void PreviewStart(IHoverable hoverable)
    {
        var card = hoverable.DisplayCard;
        if (card == null) { return; }
        CardPreview.Setup(card);
        CardPreview.CanPlayIndicator.gameObject.SetActive(false);
        CardPreview.gameObject.SetActive(true);
    }

    internal void PreviewEnd()
    {
        CardPreview.gameObject.SetActive(false);
    }

    internal void HoverPreviewStart(IHoverable hoverable)
    {
        var card = hoverable.DisplayCard;
        if (card == null) { return; }
        HoverCardPreviewObject.gameObject.SetActive(true);
        HoverCardPreview.Setup(card);
        HoverCardPreview.CanPlayIndicator.gameObject.SetActive(false);
        HoverCardPreview.gameObject.SetActive(true);
    }

    internal void HoverPreviewMove(Minion minion)
    {
    }

    internal void HoverPreviewEnd()
    {
        HoverCardPreviewObject.gameObject.SetActive(false);
        HoverCardPreview.gameObject.SetActive(false);
    }

    public void OpenSettings_Clicked()
	{
        GameSettingsScreen.gameObject.SetActive(true);
        GameSettingsScreen.Open();
    }
}
