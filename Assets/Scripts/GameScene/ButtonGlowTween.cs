using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class ButtonGlowTween : MonoBehaviour
{
    public Image GlowImage;
    public Color ColorA = Color.cyan;
    public Color ColorB = Color.magenta;

    void Start()
    {
        // Scale pulse
        GlowImage.rectTransform
            .DOScale(0.8f, 1.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Color loop
        GlowImage.color = ColorA;
        GlowImage
            .DOColor(ColorB, 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);

        // Optional alpha pulse layered on top
        //GlowImage
        //    .DOFade(0.8f, 1.5f)
        //    .From(0.3f)
        //    .SetEase(Ease.InOutSine)
        //    .SetLoops(-1, LoopType.Yoyo);
    }
}
