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
    public GameObject TriggeredEffectParticlePrefab;

    public GameResultScreen GameResultScreen;
    public GameSettings GameSettingsScreen;
    public GameObject SettingsButton;

	private void Start()
	{
        Message.gameObject.SetActive(false);
        GameResultScreen.gameObject.SetActive(false);
        GameSettingsScreen.gameObject.SetActive(false);
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

    internal void PreviewStart(IHoverable hoverable)
    {
        var card = hoverable.GetDisplayCard();
        if (card == null) { return; }
        CardPreview.Setup(card);
        CardPreview.gameObject.SetActive(true);
    }

	internal void PreviewEnd()
    {
        CardPreview.gameObject.SetActive(false);
    }

    public void OpenSettings_Clicked()
	{
        GameSettingsScreen.gameObject.SetActive(true);
        GameSettingsScreen.Open();
    }
}
