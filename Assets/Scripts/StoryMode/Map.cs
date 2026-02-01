using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
	[Header("Map")]
	public GameObject MapObject;
	public List<MapLocation> MapLocations;
	public MapLocation SelectedLocation;

	// location preview
	public TextMeshProUGUI RegionText;
	public TextMeshProUGUI RegionDescriptionText;

	[Header("Dungeon Picker")]
	public GameObject LocationObject;
	public Transform Container;
	public BattleGridButton BattleGridButtonPrefab;
	public BattlePreview BattlePreview;
	public TextMeshProUGUI DungeonText;
	public Dungeon Dungeon;

	private void Awake()
	{
		foreach(var location in MapLocations)
		{
			location.ClickAction = SelectLocation;
		}
	}

	internal void ShowRegionPicker()
	{
		MapObject.gameObject.SetActive(true);
		LocationObject.gameObject.SetActive(false);
		SelectLocation(MapLocations[0]);
	}

	public void SelectLocation(MapLocation mapLocation)
	{
		SelectedLocation = mapLocation;

		foreach(var location in MapLocations)
		{
			location.SetSelected(location == SelectedLocation);
		}

		RegionText.text = mapLocation.MapRegionDefinition.Name;
		RegionDescriptionText.text = mapLocation.MapRegionDefinition.Description;
	}

	public void Enter_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			MapObject.gameObject.SetActive(false);
			LocationObject.gameObject.SetActive(true);
			InitializeGridButtons();
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

	private void InitializeGridButtons()
	{
		foreach (Transform child in Container)
		{
			Destroy(child.gameObject);
		}

		var dungeons = SelectedLocation.MapRegionDefinition.Dungeons;
		foreach (var data in dungeons)
		{
			var newButton = Instantiate(BattleGridButtonPrefab, Container);
			newButton.Setup(data);
			newButton.ClickAction = BattleGridButton_Clicked;
		}

		BattleGridButton_Clicked(null);
		DungeonText.text = $"{SelectedLocation.MapRegionDefinition.Name} Dungeons";
	}

	public void BattleGridButton_Clicked(StoryModeDungeonDefinition data)
	{
		if (data == null)
		{
			BattlePreview.gameObject.SetActive(false);
		}
		else
		{
			BattlePreview.gameObject.SetActive(true);
			BattlePreview.Setup(data);
		}
	}

	public void DungeonBack_Clicked()
	{
		Common.Instance.SceneTransition.DoTransition(() =>
		{
			MapObject.gameObject.SetActive(true);
			LocationObject.gameObject.SetActive(false);
		});
	}

	public void DungeonEnter_Clicked()
	{
		Common.Instance.SceneTransition.DoDoorTransition(() =>
		{
			var data = BattlePreview.GetData();
			MapObject.gameObject.SetActive(false);
			LocationObject.gameObject.SetActive(false);

			GameSaveData gameSaveData = Common.Instance.SaveManager.SaveData.GameSaveData;
			gameSaveData.StorySaveData.CurrentDungeon = new DungeonSaveData()
			{
				Title = $"{SelectedLocation.MapRegionDefinition.Name} {data.Description} Dungeon",
				Lives = 1,
				Wins = 0,
				MaxWins = 2,
				Exited = false,
				ID = data.DungeonID,
			};

			Dungeon.gameObject.SetActive(true);
			Dungeon.Setup();
		});
	}
}
