using System;
using UnityEngine;

public class SwitchButton : MonoBehaviour, IButtonSoundState
{
	public bool IsOn;

	public GameObject OnObject;
	public GameObject OffObject;

	public bool IsActive => IsOn;

	public event Action<bool> OnSwitch;

	public void Toggle()
	{
		SetIsOn(!IsOn);
	}

	public void SetIsOn(bool isOn)
	{
		if (IsOn == isOn)
			return;

		IsOn = isOn;

		UpdateUI();
		OnSwitch?.Invoke(IsOn);
	}

	private void UpdateUI()
	{
		OnObject.gameObject.SetActive(IsOn);
		OffObject.gameObject.SetActive(!IsOn);
	}
}
