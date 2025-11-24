using System;
using UnityEngine;

public class HeroIndicator : MonoBehaviour
{
	public GameObject ActiveIndicator;
	public DeckCard DeckCard;

	public Action<DeckCard> SetAsHeroAction { get; internal set; }

	public void OnClick()
	{
		SetAsHeroAction?.Invoke(DeckCard);
	}
}
