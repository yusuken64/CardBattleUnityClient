using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    public PlayableDirector director;
    public GameObject skipIndicator;
    public Image skipProgressImage;

    public static string ReturnScreenName;
    public static Func<bool> ExitAction;
	private static bool exiting;
	private float holdTimer;
    private const float holdDuration = 2f;

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

    private void Update()
    {
        if (exiting) return;
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.isPressed)
        {
            skipIndicator.gameObject.SetActive(true);
            holdTimer += Time.deltaTime;

            float progress = Mathf.Clamp01(holdTimer / holdDuration);
            skipProgressImage.fillAmount = progress;

            if (holdTimer >= holdDuration)
            {
                ExitScene();
            }
        }
        else
        {
            holdTimer = 0f;
            skipProgressImage.fillAmount = 0f;
            skipIndicator.gameObject.SetActive(false);
        }
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
        if (exiting) return;
        exiting = true;

        var exitAction = ExitAction;
        ExitAction = null;

        var returnScreenName = IntroCutscene.ReturnScreenName;
        IntroCutscene.ReturnScreenName = null;

        // If ExitAction handled the exit, STOP.
        if (exitAction?.Invoke() == true)
            return;

        string sceneToLoad = string.IsNullOrWhiteSpace(returnScreenName)
            ? "Main"
            : returnScreenName;

        string capturedScene = sceneToLoad;

        Common.Instance.SceneTransition.DoTransition(() =>
        {
            SceneManager.LoadScene(capturedScene);
        });
    }
}
