using System;
using UnityEngine;

public class SwitchButton : MonoBehaviour, IButtonSoundState
{
	public bool IsOn;

	public GameObject OnObject;
	public GameObject OffObject;

	public bool IsActive => IsOn;

	public void Toggle()
	{
		SetIsOn(!IsOn);
	}

	public void SetIsOn(bool isOn)
	{
		IsOn = isOn;
		UpdateUI();
	}

	private void UpdateUI()
	{
		OnObject.gameObject.SetActive(IsOn);
		OffObject.gameObject.SetActive(!IsOn);
	}
}
