using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class VideoSettingsManager : MonoBehaviour
{
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown displayModeDropdown;
    public int defaultWidth = 1920;
    public int defaultHeight = 1080;
    private Vector2Int[] availableResolutions;
    private bool initialized;
    private bool updating;

    private void Awake()
    {
        var platform = Application.platform;
        if (!Application.isEditor && platform != RuntimePlatform.WindowsPlayer &&
            platform != RuntimePlatform.OSXPlayer && platform != RuntimePlatform.LinuxPlayer)
            gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        resolutionDropdown.onValueChanged.AddListener(Changed);
        displayModeDropdown.onValueChanged.AddListener(Changed);
        if (initialized) RefreshControls();
    }

    private void OnDisable()
    {
        resolutionDropdown.onValueChanged.RemoveListener(Changed);
        displayModeDropdown.onValueChanged.RemoveListener(Changed);
    }

    private void Start() => InitializeVideo();
    private void Changed(int value)
    {
        if (!updating && initialized) ApplySettings();
    }

    // Common.Start calls this even before the settings panel opens.
    public void InitializeVideo()
    {
        if (initialized) return;
        updating = true;
        var desktop = Screen.currentResolution;
        availableResolutions = BuildResolutionOptions(
            Screen.resolutions.Select(r => new Vector2Int(r.width, r.height)),
            new Vector2Int(desktop.width, desktop.height), new Vector2Int(Screen.width, Screen.height));
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(availableResolutions.Select(r =>
            $"{r.x} x {r.y}" + (r.x == defaultWidth && r.y == defaultHeight ? " (Recommended)" : "")).ToList());
        displayModeDropdown.ClearOptions();
        displayModeDropdown.AddOptions(new List<string> { "Fullscreen", "Windowed", "Borderless" });
        initialized = true;
        LoadSettings(false);
        updating = false;
        ApplySettings();
    }

    public static Vector2Int[] BuildResolutionOptions(IEnumerable<Vector2Int> supported, Vector2Int desktop, Vector2Int current)
    {
        var sizes = supported.Where(r => r.x > 0 && r.y > 0).ToList();
        if (desktop.x <= 0 || desktop.y <= 0) desktop = new Vector2Int(1920, 1080);
        sizes.Add(desktop);
        if (current.x > 0 && current.y > 0) sizes.Add(current);
        // Window sizes need not appear in the monitor's exclusive-fullscreen list.
        foreach (var size in new[] { new Vector2Int(1280, 720), new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080), new Vector2Int(2560, 1440), new Vector2Int(3840, 2160) })
            if (size.x <= desktop.x && size.y <= desktop.y) sizes.Add(size);
        return sizes.Distinct().OrderBy(r => r.x).ThenBy(r => r.y).ToArray();
    }

    public static int FindResolutionIndex(IReadOnlyList<Vector2Int> sizes, Vector2Int requested)
    {
        int best = 0;
        long distance = long.MaxValue;
        for (int i = 0; i < sizes.Count; i++)
        {
            long dx = (long)sizes[i].x - requested.x;
            long dy = (long)sizes[i].y - requested.y;
            long candidate = dx * dx + dy * dy;
            if (candidate < distance) { best = i; distance = candidate; }
        }
        return best;
    }

    public void ApplySettings()
    {
        if (!initialized) { InitializeVideo(); return; }
        var selected = availableResolutions[Mathf.Clamp(resolutionDropdown.value, 0, availableResolutions.Length - 1)];
        var mode = Mathf.Clamp(displayModeDropdown.value, 0, 2);
        if (mode == 0)
        {
            var exclusiveModes = Screen.resolutions.Select(r => new Vector2Int(r.width, r.height))
                .Where(r => r.x > 0 && r.y > 0).Distinct().ToArray();
            if (exclusiveModes.Length > 0)
            {
                selected = exclusiveModes[FindResolutionIndex(exclusiveModes, selected)];
                resolutionDropdown.SetValueWithoutNotify(FindResolutionIndex(availableResolutions, selected));
            }
        }
        var fullscreen = mode == 0 ? FullScreenMode.ExclusiveFullScreen :
            mode == 2 ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        // Borderless follows the desktop; retain the user's size for other modes.
        var desktop = Screen.currentResolution;
        int width = mode == 2 && desktop.width > 0 ? desktop.width : selected.x;
        int height = mode == 2 && desktop.height > 0 ? desktop.height : selected.y;
        Screen.SetResolution(width, height, fullscreen);
        RefreshControls();
        SaveSettings();
    }

    private void RefreshControls()
    {
        resolutionDropdown.interactable = displayModeDropdown.value != 2;
        resolutionDropdown.RefreshShownValue();
        displayModeDropdown.RefreshShownValue();
    }

    public void SaveSettings()
    {
        if (!initialized) return;
        var size = availableResolutions[Mathf.Clamp(resolutionDropdown.value, 0, availableResolutions.Length - 1)];
        PlayerPrefs.SetInt("ResolutionWidth", size.x);
        PlayerPrefs.SetInt("ResolutionHeight", size.y);
        PlayerPrefs.SetInt("DisplayMode", Mathf.Clamp(displayModeDropdown.value, 0, 2));
        PlayerPrefs.Save();
    }

    public void LoadSettings(bool applySettings = true)
    {
        if (!initialized) { InitializeVideo(); return; }
        // The old index referred to a 1080p-only list, not this resolution list.
        var requested = new Vector2Int(PlayerPrefs.GetInt("ResolutionWidth", defaultWidth),
            PlayerPrefs.GetInt("ResolutionHeight", defaultHeight));
        resolutionDropdown.SetValueWithoutNotify(FindResolutionIndex(availableResolutions, requested));
        displayModeDropdown.SetValueWithoutNotify(Mathf.Clamp(PlayerPrefs.GetInt("DisplayMode", 1), 0, 2));
        RefreshControls();
        if (applySettings) ApplySettings();
    }

    public void ResetToDefaults()
    {
        if (!initialized) InitializeVideo();
        resolutionDropdown.SetValueWithoutNotify(FindResolutionIndex(availableResolutions, new Vector2Int(defaultWidth, defaultHeight)));
        displayModeDropdown.SetValueWithoutNotify(1);
        ApplySettings();
    }
}
