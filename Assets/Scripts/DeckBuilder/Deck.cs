using System.Collections.Generic;
using UnityEngine;

public class Deck
{
	public string Title;

	[SerializeReference]
	public CardDefinition HeroCard;

	[SerializeReference]
	public List<CardDefinition> Cards;
}
