using CardBattleEngine;
using UnityEngine;

public class Card : MonoBehaviour
{
    public string CardName => this.name;

    public bool RequiresTarget;
    public CardType CardType;
}
