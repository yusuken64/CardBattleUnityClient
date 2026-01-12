using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class IntroCutscene : MonoBehaviour
{
    public PlayableDirector director;

	public static string ReturnScreenName;

	private void OnEnable()
    {
        director.stopped += OnTimelineStopped;
    }

    private void OnDisable()
    {
        director.stopped -= OnTimelineStopped;
    }

	private void Start()
    {
        DOVirtual.DelayedCall(3f, () =>
        {
            director.Play();
			DOVirtual.DelayedCall((float)director.duration + 3f, () =>
			{
				ExitScene();
			});
		});
    }

	private void OnTimelineStopped(PlayableDirector pd)
    {
        // This will be called when the timeline finishes or is stopped
  //      Debug.Log("Timeline has ended!");
		//ExitScene();
		// Your reaction code here
	}

	private static void ExitScene()
	{
		string returnScreenName = IntroCutscene.ReturnScreenName;
		if (String.IsNullOrWhiteSpace(returnScreenName))
		{
			Common.Instance.SceneTransition.DoTransition(() =>
			{
				SceneManager.LoadScene("Main");
			});
		}
		else
		{
			IntroCutscene.ReturnScreenName = null;
			Common.Instance.SceneTransition.DoTransition(() =>
			{
				SceneManager.LoadScene(returnScreenName);
			});
		}
	}
}
