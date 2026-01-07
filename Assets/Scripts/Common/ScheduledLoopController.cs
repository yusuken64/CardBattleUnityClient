using UnityEngine;

public class ScheduledLoopController : MonoBehaviour
{
	public AudioSource introSource;
	public AudioSource loopSource;

	private bool isLooping = false;

	public void Play(AudioClip introClip, AudioClip loopClip)
	{
		if (introClip == null || loopClip == null)
		{
			Debug.LogWarning("AudioClip is null!");
			return;
		}

		introSource.Stop();
		loopSource.Stop();

		introSource.clip = introClip;
		loopSource.clip = loopClip;

		introSource.Play();
		loopSource.loop = true;
		loopSource.PlayScheduled(AudioSettings.dspTime + introSource.clip.length);
		isLooping = true;
	}

	public void StopLoop()
	{
		if (isLooping)
		{
			loopSource.Stop();
			isLooping = false;
		}
	}
}