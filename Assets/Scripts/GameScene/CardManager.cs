using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
	public List<CardDefinition> Cards;

	//TODO change to get sprite by ID;
	internal CardDefinition GetCardByName(string name)
	{
		return Cards.FirstOrDefault(x => x.CardName == name);
	}
}
