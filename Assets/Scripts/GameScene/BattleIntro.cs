using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleIntro : MonoBehaviour
{
    public Image HeroPortrait1;
    public TextMeshProUGUI Hero1NameText;
    public TextMeshProUGUI Hero1DeckText;

    public Image HeroPortrait2;
    public TextMeshProUGUI Hero2NameText;
    public TextMeshProUGUI Hero2DeckText;

    public GameObject VSObject;
    public Image BG;

    public float IntroDelay = 3f;
    public float FadeDuration = 0.5f;
    public float MoveDuration = 0.75f;

    public Transform Hero1Target;
    public Transform Hero2Target;

	public void Setup(Deck playerDeck, Deck enemyDeck)
	{
        HeroPortrait1.sprite = playerDeck.HeroCard.Sprite;
        Hero1DeckText.text = playerDeck.Title;

        HeroPortrait2.sprite = enemyDeck.HeroCard.Sprite;
        Hero2DeckText.text = enemyDeck.Title;
    }

    internal void DoIntro(Action startGameCallback)
    {
        StartCoroutine(DoIntroRoutine(startGameCallback));
    }

    public IEnumerator DoIntroRoutine(Action startGameCallback)
    {
        VSObject.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        VSObject.transform
            .DORotate(new Vector3(0f, 0f, 0f), 0.5f)
            .SetEase(Ease.OutCubic);

        this.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(3.0f);
        var seq = DOTween.Sequence();

        // Fade out deck text
        seq.Join(Hero1DeckText.DOFade(0f, FadeDuration));
        seq.Join(Hero1NameText.DOFade(0f, FadeDuration));

        seq.Join(Hero2DeckText.DOFade(0f, FadeDuration));
        seq.Join(Hero2NameText.DOFade(0f, FadeDuration));

        // Move portraits to their battle positions
        seq.Join(HeroPortrait1.rectTransform
            .DOMove(Hero1Target.position, MoveDuration)
            .SetEase(Ease.OutCubic));
        seq.Join(HeroPortrait1.transform.DOScale(Hero1Target.localScale, MoveDuration));

        seq.Join(HeroPortrait2.rectTransform
            .DOMove(Hero2Target.position, MoveDuration)
            .SetEase(Ease.OutCubic));
        seq.Join(HeroPortrait2.transform.DOScale(Hero2Target.localScale, MoveDuration));

        seq.Join(VSObject.transform
            .DORotate(new Vector3(0f, -90f, 0f), 0.5f)
            .SetEase(Ease.OutCubic));

        seq.Join(BG.DOFade(0f, FadeDuration));

        seq.OnComplete(() =>
        {
            startGameCallback?.Invoke();
            this.gameObject.SetActive(false);
        });

    }
}
