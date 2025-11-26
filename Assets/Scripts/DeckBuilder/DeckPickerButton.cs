using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckPickerButton : MonoBehaviour
{
	public Image DeckImage;
	public TextMeshProUGUI TitleText;

	public Action<DeckPickerButton> DeckPickedAction { get; internal set; }
	public Deck Deck { get; private set; }

	internal void Setup(Deck deck)
	{	
		Deck = deck;
		UpdateUI();
	}

	public void Click()
	{
		DeckPickedAction?.Invoke(this);
	}

	internal void UpdateUI()
	{
		TitleText.text = Deck.Title;
		//DeckImage.sprite = Deck.HeroCard.Sprite;
	}
}
