using System;
using UnityEngine;

public class FloatingCardPreview : MonoBehaviour
{
	public Card PreviewCard;

	private float lockedX;
	private RectTransform previewRect;
	private RectTransform canvasRect;
	private Canvas canvas;

	void Awake()
	{
		previewRect = PreviewCard.GetComponent<RectTransform>();
		canvas = GetComponentInParent<Canvas>();
		canvasRect = canvas.transform as RectTransform;
	}

	private void Start()
	{
		PreviewEnd();
	}

	internal void PreviewStart(CardDefinition cardDefinition, Vector2 screenPos)
	{
		gameObject.SetActive(true);
		PreviewCard.Setup(cardDefinition.CreateCard());
	}

	internal void PreviewEnd()
	{
		this.gameObject.SetActive(false);
	}
}
