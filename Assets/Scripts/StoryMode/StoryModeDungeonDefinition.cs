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
	public string DungeonName;
	public string Description;

	public int MaxWins;

	[SerializeReference]
	public List<StoryModeDungeonEncounterDefinition> StoryModeDungeonEncounterDefinitions;
}

