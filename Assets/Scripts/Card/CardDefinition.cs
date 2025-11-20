using UnityEngine;

public abstract class CardDefinition : ScriptableObject
{
    public string CardName;
    public string ID;
    public Sprite Sprite;
    public int Cost = 1;

	internal abstract CardBattleEngine.Card CreateCard();
}
