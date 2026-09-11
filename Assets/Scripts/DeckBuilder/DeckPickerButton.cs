using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeckPickerButton : MonoBehaviour, IPointerClickHandler
{
	public Image DeckImage;
	public TextMeshProUGUI TitleText;
	public GameObject ActiveIndicator;
	public HoldClickableButton HoldClickableButton;
	private DeckSaveData _deckData;

	public Action<DeckPickerButton> DeckPickedAction { get; internal set; }
	public Action<DeckPickerButton> DeckSelectedAction { get; internal set; }
	public Action<DeckPickerButton> DeleteRequestedAction { get; internal set; }
	public bool SelectMode { get; internal set; }
	public Deck Deck { get; private set; }
	public string DeckID => _deckData?.ID;

	private void OnEnable()
	{
		if (HoldClickableButton != null)
			HoldClickableButton.OnHoldClicked += HandleHoldClicked;
	}

	private void OnDisable()
	{
		if (HoldClickableButton != null)
			HoldClickableButton.OnHoldClicked -= HandleHoldClicked;
	}

	internal void Setup(DeckSaveData deckData)
	{
		_deckData = deckData;
		ResetData();
	}

	public void ResetData()
	{
		Deck = _deckData.ToDeck();
		UpdateUI();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			HandleLeftClick();
		}
		else if (eventData.button == PointerEventData.InputButton.Right)
		{
			DeleteRequestedAction?.Invoke(this);
		}
	}

	private void HandleLeftClick()
	{
		ResetData();
		if (SelectMode)
			DeckSelectedAction?.Invoke(this);
		else
			DeckPickedAction?.Invoke(this);
	}

	private void HandleHoldClicked()
	{
		DeleteRequestedAction?.Invoke(this);
	}

	public void SetActiveHighlight(bool isActive)
	{
		if (ActiveIndicator != null)
			ActiveIndicator.SetActive(isActive);
	}

	internal void UpdateUI()
	{
		TitleText.text = Deck.Title;
		DeckImage.sprite = Common.Instance.CardManager.GetSpriteByCardID(Deck.HeroCard?.ID);
	}
}
