using System;
using UnityEngine;

public class MapLocation : MonoBehaviour
{
	public GameObject FocusObject;

	public Action<MapLocation> ClickAction { get; internal set; }

	internal void SetSelected(bool selected)
	{
		FocusObject.gameObject.SetActive(selected);
	}

	public void Click()
	{
		ClickAction?.Invoke(this);
	}
}
