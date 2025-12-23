using System;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public GameObject AudioSettings;
    public GameObject DisplaySettings;

    public void Close_Clicked()
    {
        Common.Instance.SaveManager.Save();
        this.gameObject.SetActive(false);
    }

	internal void SetToAudioSettings()
	{
		AudioSettings.gameObject.SetActive(true);
		DisplaySettings.gameObject.SetActive(false);
	}

	internal void SetToDisplaySettings()
	{
		AudioSettings.gameObject.SetActive(false);
		DisplaySettings.gameObject.SetActive(true);
	}
}
