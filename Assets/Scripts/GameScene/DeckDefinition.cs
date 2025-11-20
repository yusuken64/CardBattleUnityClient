using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Game/Cards/Deck Definition"
)]
public class DeckDefinition : ScriptableObject
{
    public List<MinionCardDefinition> Cards;
}
