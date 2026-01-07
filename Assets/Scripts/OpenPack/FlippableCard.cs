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

	public Action FlipComplete;

	public Card DisplayCard;

	public AudioClip FlipCard;
	public AudioClip JumpCard;

	private void Start()
	{
		SetToBack();
	}

	private void SetToBack()
	{
		Back.SetActive(true);
		Front.SetActive(false);
		flipped = false;
	}

	public void Flip()
	{
		if (flipped || !CanFlip) { return; }
		flipped = true;
		// Ensure initial visibility
		Back.SetActive(true);
		Front.SetActive(false);

		Common.Instance.AudioManager.PlayUISound(FlipCard);

		// First half: rotate to 90 degrees
		transform.DOLocalRotate(
			new Vector3(0f, 90f, 0f),
			FlipDuration * 0.5f
		).SetEase(FlipEase)
		.OnComplete(() =>
		{
			// Swap visible side at midpoint
			Back.SetActive(false);
			Front.SetActive(true);

			// Second half: rotate back to 0
			transform.DOLocalRotate(
				Vector3.zero,
				FlipDuration * 0.5f
			).SetEase(FlipEase)
			.OnComplete(() => FlipComplete?.Invoke());
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
