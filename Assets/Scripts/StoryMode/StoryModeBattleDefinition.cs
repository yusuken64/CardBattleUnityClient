using CardBattleEngine;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "NewBattle",
	menuName = "Game/Story/Battle"
)]
public class StoryModeBattleDefinition : ScriptableObject
{
	public string LevelID;
	public Sprite BattleImage;
	public string Description;
	public int Health = 30;
	public DeckDefinition Deck;

	[SerializeReference]
	public List<StoryBattleModifier> StoryBattleModifiers;
}
