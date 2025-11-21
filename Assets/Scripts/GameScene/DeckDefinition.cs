using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Game/Cards/Deck Definition"
)]
public class DeckDefinition : ScriptableObject
{
    public HeroPowerDefinition HeroPower;
    public List<CardDefinition> Cards;
}
