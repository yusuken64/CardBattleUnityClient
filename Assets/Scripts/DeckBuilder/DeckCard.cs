using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour,
	IPointerEnterHandler,
	IPointerExitHandler,
	IBeginDragHandler,
	IDragHandler,
	IEndDragHandler
{
	public TextMeshProUGUI ManaText;
	public TextMeshProUGUI NameText;
	public Image CardImage;

	public CardDefinition CardDefinition;
	public Action<DeckCard> RemoveCardFromDeckAction;
	public Action<DeckCard> SetAsHeroAction;

	public HeroIndicator HeroIndicator;

	public void Setup(
		CardDefinition cardDefinition,
		Action<DeckCard> removeCardFromDeck,
		Action<DeckCard> setAsHeroAction)
	{
		this.CardDefinition = cardDefinition;
		this.RemoveCardFromDeckAction = removeCardFromDeck;
		this.SetAsHeroAction = setAsHeroAction;

		ManaText.text = cardDefinition.Cost.ToString();
		NameText.text = cardDefinition.CardName.ToString();
		CardImage.sprite = cardDefinition.Sprite;

		HeroIndicator.SetAsHeroAction = setAsHeroAction;
		HeroIndicator.DeckCard = this;
		
		FloatingCardPreview = FindFirstObjectByType<FloatingCardPreview>(FindObjectsInactive.Include);
	}

	public void OnClick()
	{
		RemoveCardFromDeckAction?.Invoke(this);
		HidePreview();
	}

	public void SetAsHero_Click()
	{
		SetAsHeroAction?.Invoke(this);
	}

	internal void SetAsHero(bool activeHero)
	{
		HeroIndicator.ActiveIndicator.SetActive(activeHero);
	}

	private ScrollRect scrollRect;

	void Awake()
	{
		scrollRect = GetComponentInParent<ScrollRect>();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		scrollRect?.OnBeginDrag(eventData);
	}

	private bool dragged;

	public void OnPointerUp(PointerEventData eventData)
	{
		if (dragged)
			eventData.Use();

		dragged = false;
	}

	public float hoverDelay = 0.15f;

	private bool hovering;
	private bool dragging;
	private float hoverStartTime;
	private Vector2 lastPointerPosition;

	public FloatingCardPreview FloatingCardPreview { get; private set; }

	void Update()
	{
		if (hovering && !dragging)
		{
			if (Time.unscaledTime - hoverStartTime >= hoverDelay)
			{
				ShowPreview();
				hovering = false; // fire once
			}
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		lastPointerPosition = eventData.position;
		hovering = true;
		dragging = false;
		hoverStartTime = Time.unscaledTime;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		hovering = false;
		HidePreview();
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragging = true;
		hovering = false;
		HidePreview();
		scrollRect?.OnBeginDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		lastPointerPosition = eventData.position;
		scrollRect?.OnDrag(eventData);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		dragging = false;
		scrollRect?.OnEndDrag(eventData);
	}

	void ShowPreview()
	{
		Debug.Log("Show preview");

		FloatingCardPreview?.PreviewStart(this.CardDefinition, lastPointerPosition);
		hoverStartTime = Time.unscaledTime;
	}

	void HidePreview()
	{
		Debug.Log("Hide preview");

		FloatingCardPreview?.PreviewEnd();
	}
}
