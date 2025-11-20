using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : MonoBehaviour
{
	public List<MinionCardDefinition> MinionCards;

	internal MinionCardDefinition GetCardByName(string name)
	{
		return MinionCards.FirstOrDefault(x => x.CardName == name);
	}
}
