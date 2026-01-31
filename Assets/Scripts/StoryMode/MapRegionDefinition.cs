using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
	fileName = "NewBattle",
	menuName = "Game/Story/MapRegion"
)]
public class MapRegionDefinition : ScriptableObject
{
	public string RegionID;
	public string Name;
	public string Description;

	[SerializeReference]
	public List<StoryModeDungeonDefinition> Dungeons;
}

