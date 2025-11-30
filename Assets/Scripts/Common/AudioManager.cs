using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public AudioMixer AudioMixer;

	public AudioSource MusicAudioSource;
	public List<AudioSource> Sounds;
	public List<AudioSource> UISounds;
	private int _index;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(this);
		}
        else
        {
			Destroy(this.gameObject);
        }
	}

	public void PlayMusic(AudioClip musicClip, bool loop = true)
	{
		if (MusicAudioSource.clip == musicClip && MusicAudioSource.isPlaying && MusicAudioSource.loop == loop)
		{
			return;
		}

		MusicAudioSource.clip = musicClip;
		MusicAudioSource.loop = loop;
		MusicAudioSource.Play();
	}

	public void StopMusc()
	{
		MusicAudioSource.Stop();
	}

	internal void PlayClip(AudioClip stinger)
	{
		MusicAudioSource.clip = stinger;
		MusicAudioSource.loop = false;
		MusicAudioSource.Play();
	}

	internal void PlaySound(AudioClip clip)
	{
		_index = (_index + 1) % Sounds.Count;
		var source = Sounds[_index];
		source.clip = clip;
		source.loop = false;
		source.pitch = 1;
		source.Play();
	}

	internal void PlayUISound(AudioClip clip)
	{
		_index = (_index + 1) % UISounds.Count;
		var source = UISounds[_index];
		source.clip = clip;
		source.loop = false;
		source.pitch = 1;
		source.Play();
	}

	internal void PlaySoundWithRandomPitch(AudioClip clip, float minPitch = 0.5f, float maxPitch = 1.5f)
	{
		_index = (_index + 1) % Sounds.Count;
		var source = Sounds[_index];
		source.clip = clip;
		source.loop = false;

		// Set random pitch between the specified min and max
		source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);

		source.Play();
	}

	public float GetVolumeSliderValue(string volumeParameter)
	{
		if (AudioMixer.GetFloat(volumeParameter, out float currentVolume))
		{
			return Mathf.Pow(10, currentVolume / 20); // Convert dB to linear (0-1)
		}
		return 1f; // Default to full volume if not found
	}

	public void OnVolumeSliderChanged(string volumeParameter, float sliderValue)
	{
		float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20; // Prevent log errors
		AudioMixer.SetFloat(volumeParameter, volume);

		switch (volumeParameter)
		{
			case "MIXVol":
				//SingletonSaveData.Instance.SaveData.ApplicationData.MIXVolume = sliderValue;
				break;
			case "BGMVol":
				//SingletonSaveData.Instance.SaveData.ApplicationData.BGMVolume = sliderValue;
				break;
			case "VOCVol":
				//SingletonSaveData.Instance.SaveData.ApplicationData.VOCVolume = sliderValue;
				break;
			case "SFXVol":
				//SingletonSaveData.Instance.SaveData.ApplicationData.SFXVolume = sliderValue;
				break;
			default:
				break;
		}
	}
	//public void ApplicationInitialized(SaveData saveData)
	//{
	//	OnVolumeSliderChanged("MIXVol", saveData.ApplicationData.MIXVolume);
	//	OnVolumeSliderChanged("BGMVol", saveData.ApplicationData.BGMVolume);
	//	OnVolumeSliderChanged("VOCVol", saveData.ApplicationData.VOCVolume);
	//	OnVolumeSliderChanged("SFXVol", saveData.ApplicationData.SFXVolume);
	//}
}
