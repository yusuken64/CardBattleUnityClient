using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameSaveData
{
	[SerializeReference]
	public List<Deck> Decks = new();
}
