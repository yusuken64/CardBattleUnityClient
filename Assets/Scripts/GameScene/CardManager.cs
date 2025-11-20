using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
	public List<CardDefinition> Cards;

	internal CardDefinition GetCardByName(string name)
	{
		return Cards.FirstOrDefault(x => x.CardName == name);
	}
}
