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

		// Front can itself be a self-contained Card that wraps its own FlippableCard (e.g. pack-opening
		// and story-intro cards nest a full Card prefab as their Front). Cascade the reveal so that
		// content is never left showing its own inner "back" once this outer card is revealed.
		DisplayCard?.FlippableCard?.SetToFront();
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

			// See SetToFront() - cascade the reveal to a nested Card's own FlippableCard, if any.
			DisplayCard?.FlippableCard?.SetToFront();

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

		var flippable = GetOutermostFlippableCard(hit.transform);
		if (flippable != null)
		{
			flippable.Flip();
		}
	}

	// A hit collider can sit on a GameObject that has its own FlippableCard (e.g. the nested Card's
	// BoxCollider2D, co-located with that Card's own self-contained FlippableCard) while ALSO being
	// nested inside an outer, wrapping FlippableCard (e.g. a pack-opening chest card). Clicking should
	// always flip the outermost one - that's the actual interactive reveal-ceremony affordance; any
	// inner FlippableCard is just the wrapped content's own (here, unused) standalone flip capability.
	private static FlippableCard GetOutermostFlippableCard(Transform start)
	{
		FlippableCard outermost = null;
		for (var current = start; current != null; current = current.parent)
		{
			var candidate = current.GetComponent<FlippableCard>();
			if (candidate != null)
			{
				outermost = candidate;
			}
		}
		return outermost;
	}

	internal void Setup(CardDefinition cardDefinition)
	{
		DisplayCard.Setup(cardDefinition.CreateCard());
	}
}