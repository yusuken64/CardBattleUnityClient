using System;
using System.Collections.Generic;
using UnityEngine;

public class MapLocation : MonoBehaviour
{
	public GameObject FocusObject;
	public MapRegionDefinition MapRegionDefinition;

	public List<GameObject> Stars;

	public Action<MapLocation> ClickAction { get; internal set; }

	internal void SetSelected(bool selected)
	{
		FocusObject.gameObject.SetActive(selected);
	}

	public void Click()
	{
		ClickAction?.Invoke(this);
	}

	public void SetStars(int stars)
	{
		for (int i = 0; i < Stars.Count; i++)
		{
			Stars[i].gameObject.SetActive(i < stars);
		}
	}
}
