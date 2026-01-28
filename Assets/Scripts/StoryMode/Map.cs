using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
	public List<MapLocation> MapLocations;
	public MapLocation SelectedLocation;

	private void Awake()
	{
		foreach(var location in MapLocations)
		{
			location.ClickAction = SelectLocation;
		}
	}

	public void SelectLocation(MapLocation mapLocation)
	{
		SelectedLocation = mapLocation;

		foreach(var location in MapLocations)
		{
			location.SetSelected(location == SelectedLocation);
		}
	}

#if UNITY_EDITOR
	[ContextMenu("Find All Locations")]
	public void FindAllLocations()
	{
		MapLocations = new List<MapLocation>(GetComponentsInChildren<MapLocation>(true));
	}
#endif
}
