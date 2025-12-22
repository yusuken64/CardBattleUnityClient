using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPickerButton : MonoBehaviour
{
	public Image DeckImage;
	public TextMeshProUGUI TitleText;
	private DeckSaveData _deckData;

	public Action<DeckPickerButton> DeckPickedAction { get; internal set; }
	public Deck Deck { get; private set; }


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

	public void Click()
	{
		ResetData();
		DeckPickedAction?.Invoke(this);
	}

	internal void UpdateUI()
	{
		TitleText.text = Deck.Title;
		DeckImage.sprite = Common.Instance.CardManager.GetSpriteByCardName(Deck.HeroCard.CardName);
	}
}
