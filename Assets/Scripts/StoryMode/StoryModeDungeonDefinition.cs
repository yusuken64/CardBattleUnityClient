using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "NewDungeon",
	menuName = "Game/Story/StoryModeDungeon"
)]
public class StoryModeDungeonDefinition : ScriptableObject
{
	public string DungeonID;
	public Sprite BattleImage;
	public string Description;

	[SerializeReference]
	public List<StoryModeDungeonEncounterDefinition> StoryModeDungeonEncounterDefinition;
}

