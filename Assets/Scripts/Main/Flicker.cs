using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Flicker : MonoBehaviour
{
	public Image Gradient;

	void Start()
    {
		DoFlicker();
    }

	public void DoFlicker()
	{
		Gradient
			.DOFade(UnityEngine.Random.Range(0f, 0.02f), 0.06f)
			.SetEase(Ease.InOutSine)
			.OnComplete(DoFlicker);
	}

}
