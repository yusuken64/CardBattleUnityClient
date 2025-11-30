using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlider : MonoBehaviour
{
    public Slider Slider;
    public string VolumeParameter;

	void Start()
	{
		Slider.value = (float)(AudioManager.Instance?.GetVolumeSliderValue(VolumeParameter));
		Slider.onValueChanged.AddListener(OnVolumeSliderChanged);
	}

	public void OnVolumeSliderChanged(float sliderValue)
    {
        AudioManager.Instance?.OnVolumeSliderChanged(VolumeParameter, sliderValue);
    }
}
