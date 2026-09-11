using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FlippableCard : MonoBehaviour
{
	public GameObject Back;
	public GameObject Front;

	public float FlipDuration = 0.5f;
	public Ease FlipEase = Ease.InOutQuad;

	public LayerMask ClickableMask;
	public bool CanFlip;
	private bool flipped;

	public Action FlipMidPoint;
	public Action FlipComplete;

	public Card DisplayCard;

	public AudioClip FlipCard;
	public AudioClip JumpCard;

	// Whether Card.Setup() is allowed to flip this to the front as soon as it has data to show.
	// False for reveal-ceremony flows (pack opening, story intro) that must stay closed until
	// an explicit Flip() call, regardless of when/whether Setup() runs.
	public bool RevealOnSetup = true;

	// Runs synchronously during Instantiate(), before the instantiating caller's next line of
	// code executes - unlike Start(), which is deferred and would otherwise run after (and clobber)
	// a Setup() call made immediately following Instantiate(). This is what makes the default
	// deterministic instead of racing against every caller's Instantiate()+Setup() pattern.
	private void Awake()
	{
		SetToBack();
	}

	private bool animating;
	public bool IsAnimating => animating;

	public bool IsFlipped => flipped;

	public void SetToBack()
	{
		if (animating || flipped)
		{
			transform.DOKill();
			transform.localRotation = Quaternion.identity;
			//transform.localScale = Vector3.one;
			animating = false;
		}

		Back.SetActive(true);
		Front.SetActive(false);
		flipped = false;
	}

	public void SetToFront()
	{
		if (animating || !flipped)
		{
			transform.DOKill();
			transform.localRotation = Quaternion.identity;
			//transform.localScale = Vector3.one;
			animating = false;
		}

		Back.SetActive(false);
		Front.SetActive(true);
		flipped = true;
	}

	public void Flip()
	{
		if (flipped || !CanFlip) { return; }
		flipped = true;
		animating = true;
		// Ensure initial visibility
		Back.SetActive(true);
		Front.SetActive(false);

		Common.Instance.AudioManager.PlayUISound(FlipCard);
		transform.DOPunchScale(Vector3.one * 0.2f, 0.2f, 1, 0.5f);

		// First half: rotate to 90 degrees
		transform.DOLocalRotate(
			new Vector3(0f, 90f, 0f),
			FlipDuration * 0.5f
		).SetEase(FlipEase)
		.OnComplete(() =>
		{
			FlipMidPoint?.Invoke();
			// Swap visible side at midpoint
			Back.SetActive(false);
			Front.SetActive(true);

			// Second half: rotate back to 0
			transform.DOLocalRotate(
				Vector3.zero,
				FlipDuration * 0.5f
			).SetEase(FlipEase)
			.OnComplete(() =>
			{
				animating = false;
				FlipComplete?.Invoke();
			});
		});
	}

	void Update()
	{
		var mouse = Mouse.current;
		if (mouse == null)
			return;

		if (!mouse.leftButton.wasPressedThisFrame)
			return;

		Vector3 screenPos = mouse.position.ReadValue();
		screenPos.z = Mathf.Abs(Camera.main.transform.position.z);

		Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

		Collider2D hit = Physics2D.OverlapPoint(worldPos);
		if (hit == null)
			return;

		var flippable = hit.GetComponentInParent<FlippableCard>();
		if (flippable != null)
		{
			flippable.Flip();
		}
	}

	internal void Setup(CardDefinition cardDefinition)
	{
		DisplayCard.Setup(cardDefinition.CreateCard());
	}
}