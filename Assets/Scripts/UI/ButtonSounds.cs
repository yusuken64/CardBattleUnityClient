using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class ButtonSounds : MonoBehaviour
{
    public AudioClip HoverSound;
    public AudioClip ClickSound;
    public AudioClip UnclickableSound;

    private Button button;
	private IButtonSoundState buttonSoundState;
	private EventTrigger trigger;

    private void Awake()
    {
        button = GetComponent<Button>();
        buttonSoundState = GetComponent<IButtonSoundState>();

        // Add or get an EventTrigger for hover events
        trigger = GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = gameObject.AddComponent<EventTrigger>();
    }

    private void OnEnable()
    {
        // Add hover entry
        AddEvent(EventTriggerType.PointerEnter, OnHover);

        // Register click event
        button.onClick.AddListener(OnClick);
    }

    private void OnDisable()
    {
        // Remove click
        button.onClick.RemoveListener(OnClick);

        // Remove hover events (cleanup)
        trigger.triggers.RemoveAll(e => e.eventID == EventTriggerType.PointerEnter);
    }

    private void AddEvent(EventTriggerType type, System.Action<BaseEventData> callback)
    {
        var entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((data) => callback(data));
        trigger.triggers.Add(entry);
    }

    private void OnHover(BaseEventData data)
    {
        if (HoverSound != null && button.interactable)
            Common.Instance.AudioManager.PlayUISound(HoverSound);
    }

    private void OnClick()
    {
        if (buttonSoundState == null ||
            buttonSoundState.IsActive)
        {
            if (ClickSound != null)
                Common.Instance.AudioManager.PlayUISound(ClickSound);
        }
        else
        {
            if (UnclickableSound != null)
                Common.Instance.AudioManager.PlayUISound(UnclickableSound);
        }
    }
}

public interface IButtonSoundState
{
    public bool IsActive { get; }
}
