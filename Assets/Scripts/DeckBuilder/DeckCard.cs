using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckCard : MonoBehaviour
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
	}

	public void OnClick()
	{
		RemoveCardFromDeckAction?.Invoke(this);
	}

	public void SetAsHero_Click()
	{
		SetAsHeroAction?.Invoke(this);
	}

	internal void SetAsHero(bool activeHero)
	{
		HeroIndicator.ActiveIndicator.SetActive(activeHero);
	}
}
