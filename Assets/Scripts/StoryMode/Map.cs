using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
	public List<MapLocation> MapLocations;
	public MapLocation SelectedLocation;

	// location preview
	public TextMeshProUGUI DungeonText;

	public Dungeon Dungeon;

	private void Awake()
	{
		foreach(var location in MapLocations)
		{
			location.ClickAction = SelectLocation;
		}
	}

	internal void Setup()
	{
		SelectLocation(MapLocations[0]);
	}

	public void SelectLocation(MapLocation mapLocation)
	{
		SelectedLocation = mapLocation;

		foreach(var location in MapLocations)
		{
			location.SetSelected(location == SelectedLocation);
		}

		DungeonText.text = mapLocation.LocationName;
	}

	public void Enter_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
			gameSaveData.StorySaveData.CurrentDungeon = new DungeonSaveData()
			{
				Title = $"{SelectedLocation.LocationName} Dungeon",
				Lives = 1,
				Wins = 0,
				MaxWins = 2,
				Exited = false,
			};

			this.gameObject.SetActive(false);
			Dungeon.gameObject.SetActive(true);
			Dungeon.Setup();
		});
	}

	public void Back_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			this.gameObject.SetActive(false);
		});
	}

#if UNITY_EDITOR
	[ContextMenu("Find All Locations")]
	public void FindAllLocations()
	{
		MapLocations = new List<MapLocation>(GetComponentsInChildren<MapLocation>(true));
	}
#endif
}
