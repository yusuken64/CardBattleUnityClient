using DG.Tweening;
using UnityEngine;

public class DeckToggle : MonoBehaviour
{
	public RectTransform Deck;

	public float TweenDuration = 0.4f;
	public Vector3 OpenPosition;
	public Vector3 ClosedPosition;

	private bool IsOpen = false;
	private bool transitioning = false;


	private void Start()
	{
		CloseDeck();
	}

	public void ToggleDeckc_Click()
	{
		if (transitioning) { return; }
		if (IsOpen)
		{
			CloseDeck();
		}
		else
		{
			OpenDeck();
		}
	}

	public void OpenDeck()
	{
		Deck.DOAnchorPos(OpenPosition, TweenDuration)
			.SetEase(Ease.OutBounce)
			.OnComplete(() =>
			{
				IsOpen = true;
				transitioning = false;
			});
	}

	public void CloseDeck()
	{
		Deck.DOAnchorPos(ClosedPosition, TweenDuration)
			.SetEase(Ease.OutBounce)
			.OnComplete(() =>
			{
				IsOpen = false;
				transitioning = false;
			});
	}
}
