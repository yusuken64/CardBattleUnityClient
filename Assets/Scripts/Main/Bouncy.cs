using DG.Tweening;
using UnityEngine;

public class Bouncy : MonoBehaviour
{
    public Vector3 punch;
    public float duration;
    public int vibrato;
    public float elasticity;

    public AudioClip Sound;

    Tween idleTween;

	private void Start()
	{
        StartIdleBreathing();
    }

	public void Click()
    {
        Common.Instance.AudioManager.PlayUISoundWithRandomPitch(Sound);
        transform.localScale = Vector3.one;

        transform.DOKill();
        idleTween?.Kill();

        transform.DOPunchScale(
            punch,
            duration,
            vibrato,
            elasticity
        ).OnComplete(StartIdleBreathing);
    }

    void StartIdleBreathing()
    {
        // Subtle, slow, hypnotic
        idleTween = transform.DOScale(
                new Vector3(1.02f, 1.02f, 1f),
                3f
            )
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }
}
