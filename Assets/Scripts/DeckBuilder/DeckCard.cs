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
	IEndDragHandler,
	IPointerClickHandler
{
	public TextMeshProUGUI ManaText;
	public TextMeshProUGUI NameText;
	public Image CardImage;

	public CardDefinition CardDefinition;
	public Action<DeckCard> SetAsHeroAction;

	public HeroIndicator HeroIndicator;

	public event Action<DeckCard> CardClickAction;
	public event Action<DeckCard> CardRightClickAction;

	public void Setup(
		CardDefinition cardDefinition,
		Action<DeckCard> cardClickAction,
		Action<DeckCard> cardRightClickAction,
		Action<DeckCard> setAsHeroAction)
	{
		this.CardDefinition = cardDefinition;
		this.CardClickAction = cardClickAction;
		this.CardRightClickAction = cardRightClickAction;
		this.SetAsHeroAction = setAsHeroAction;

		ManaText.text = cardDefinition.Cost.ToString();
		NameText.text = cardDefinition.CardName.ToString();
		CardImage.sprite = cardDefinition.Sprite;

		HeroIndicator.gameObject.SetActive(cardDefinition is MinionCardDefinition);
		HeroIndicator.SetAsHeroAction = setAsHeroAction;
		HeroIndicator.DeckCard = this;
		
		FloatingCardPreview = FindFirstObjectByType<FloatingCardPreview>(FindObjectsInactive.Include);
	}

	//public void OnClick()
	//{
	//	CardClickAction?.Invoke(this);
	//	HidePreview();
	//}

	public void OnPointerClick(PointerEventData eventData)
	{
		HidePreview();
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			CardClickAction?.Invoke(this);
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			CardRightClickAction?.Invoke(this);
		}
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
