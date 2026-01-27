using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class VideoSettingsManager : MonoBehaviour
{
	[Header("UI References")]
	public TMP_Dropdown resolutionDropdown;
	public TMP_Dropdown displayModeDropdown; // 0 = Fullscreen, 1 = Windowed, 2 = Borderless

	private Resolution[] availableResolutions;

	public int defaultWidth = 1920;
	public int defaultHeight = 1080;

	void Awake()
	{
		// Only show on standalone PC builds (Windows, macOS, Linux)
		if (Application.platform != RuntimePlatform.WindowsPlayer &&
			Application.platform != RuntimePlatform.OSXPlayer &&
			Application.platform != RuntimePlatform.LinuxPlayer &&
			Application.platform != RuntimePlatform.WindowsEditor &&
			Application.platform != RuntimePlatform.OSXEditor)
		{
			gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		resolutionDropdown.onValueChanged.AddListener(Changed);
		displayModeDropdown.onValueChanged.AddListener(Changed);
	}

	private void OnDisable()
	{
		resolutionDropdown.onValueChanged.RemoveListener(Changed);
		displayModeDropdown.onValueChanged.RemoveListener(Changed);
	}

	private void Changed(int arg0)
	{
		ApplySettings();
	}

	private void Start()
	{
		resolutionDropdown.onValueChanged.RemoveListener(Changed);
		displayModeDropdown.onValueChanged.RemoveListener(Changed);

		PopulateResolutionOptions();
		PopulateDisplayModes();
		InitializeVideo();

		resolutionDropdown.onValueChanged.AddListener(Changed);
		displayModeDropdown.onValueChanged.AddListener(Changed);
	}

	public void InitializeVideo()
	{
		if (HasSavedSettings())
		{
			LoadSettings(false);              // UI + Screen reflect player choice
		}
		else
		{
			DetectCurrentSystemState();  // UI reflects OS state
		}
	}

	bool HasSavedSettings()
	{
		return PlayerPrefs.HasKey("ResolutionIndex") &&
			   PlayerPrefs.HasKey("DisplayMode");
	}

	void DetectCurrentSystemState()
	{
		// Match current resolution
		int resIndex = System.Array.FindIndex(
			availableResolutions,
			r => r.width == Screen.width &&
				 r.height == Screen.height &&
				 r.refreshRate == Screen.currentResolution.refreshRate
		);

		if (resIndex < 0)
		{
			// Fallback: match by width/height only
			resIndex = System.Array.FindIndex(
				availableResolutions,
				r => r.width == Screen.width && r.height == Screen.height
			);
		}

		if (resIndex < 0)
			resIndex = 0;

		resolutionDropdown.SetValueWithoutNotify(resIndex);

		// Match fullscreen mode
		int modeIndex = Screen.fullScreenMode switch
		{
			FullScreenMode.ExclusiveFullScreen => 0,
			FullScreenMode.Windowed => 1,
			FullScreenMode.FullScreenWindow => 2,
			_ => 0
		};

		displayModeDropdown.SetValueWithoutNotify(modeIndex);

		resolutionDropdown.RefreshShownValue();
		displayModeDropdown.RefreshShownValue();
	}

	void PopulateResolutionOptions()
	{
		//// Get valid system resolutions
		availableResolutions = Screen.resolutions
			.Where(r => r.width == defaultWidth && r.height == defaultHeight)
			.OrderBy(r => r.width)
			.ThenBy(r => r.height)
			.ToArray();

		//// Inject defaultWidthxdefaultHeight if missing
		//bool hasBaseResolution = availableResolutions.Any(r => r.width == defaultWidth && r.height == defaultHeight);
		//if (!hasBaseResolution)
		//{
		//	var customList = availableResolutions.ToList();
		//	customList.Add(new Resolution { width = defaultWidth, height = defaultHeight, refreshRate = 60 });
		//	availableResolutions = customList
		//		.OrderBy(r => r.width)
		//		.ThenBy(r => r.height)
		//		.ToArray();
		//}

		resolutionDropdown.ClearOptions();

		// Label the recommended one
		var options = availableResolutions
			.Select(r => (r.width == defaultWidth && r.height == defaultHeight)
				? $"{r.width}x{r.height} (*)"
				: $"{r.width}x{r.height}")
			.Distinct()
			.ToList();

		resolutionDropdown.AddOptions(options);

		// Get recommended resolution index
		int recommendedIndex = availableResolutions.ToList()
			.FindIndex(r => r.width == defaultWidth && r.height == defaultHeight);

		// If user already has a saved preference, use that
		bool hasSaved = PlayerPrefs.HasKey("ResolutionIndex");
		int savedIndex = hasSaved
			? PlayerPrefs.GetInt("ResolutionIndex")
			: recommendedIndex;

		resolutionDropdown.value = Mathf.Clamp(savedIndex, 0, options.Count - 1);
		resolutionDropdown.RefreshShownValue();

		// If no saved setting yet, apply the recommended one immediately
		if (!hasSaved)
		{
			Resolution recommended = availableResolutions[recommendedIndex];
			Screen.SetResolution(recommended.width, recommended.height, FullScreenMode.Windowed);
			PlayerPrefs.SetInt("ResolutionIndex", recommendedIndex);
			PlayerPrefs.Save();
		}
	}

	void PopulateDisplayModes()
	{
		displayModeDropdown.ClearOptions();
		displayModeDropdown.AddOptions(new System.Collections.Generic.List<string>
		{
			"Fullscreen",
			"Windowed",
			"Borderless"
		});

		displayModeDropdown.value = Screen.fullScreenMode switch
		{
			FullScreenMode.FullScreenWindow => 2,
			FullScreenMode.Windowed => 1,
			_ => 0
		};
		displayModeDropdown.RefreshShownValue();
	}

	public void ApplySettings()
	{
		int resIndex = resolutionDropdown.value;
		Resolution selectedRes = availableResolutions[resIndex];

		FullScreenMode mode = displayModeDropdown.value switch
		{
			0 => FullScreenMode.ExclusiveFullScreen,
			1 => FullScreenMode.Windowed,
			2 => FullScreenMode.FullScreenWindow,
			_ => FullScreenMode.ExclusiveFullScreen
		};

		Screen.SetResolution(selectedRes.width, selectedRes.height, mode);

		SaveSettings();
	}

	public void SaveSettings()
	{
		PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
		PlayerPrefs.SetInt("DisplayMode", displayModeDropdown.value);
		PlayerPrefs.Save();
	}

	public void LoadSettings(bool applySettings = true)
	{
		if (!PlayerPrefs.HasKey("ResolutionIndex") || !PlayerPrefs.HasKey("DisplayMode"))
		{
			ResetToDefaults();
			return;
		}

		int resIndex = Mathf.Clamp(PlayerPrefs.GetInt("ResolutionIndex"), 0, resolutionDropdown.options.Count - 1);
		int modeIndex = Mathf.Clamp(PlayerPrefs.GetInt("DisplayMode"), 0, displayModeDropdown.options.Count - 1);

		resolutionDropdown.value = resIndex;
		displayModeDropdown.value = modeIndex;

		if (applySettings)
		{
			ApplySettings();
		}
	}

	public void ResetToDefaults()
	{
		// Find the recommended defaultWidthxdefaultHeight resolution index
		int recommendedIndex = availableResolutions
			.ToList()
			.FindIndex(r => r.width == defaultWidth && r.height == defaultHeight);

		if (recommendedIndex < 0)
			recommendedIndex = availableResolutions.Length - 1; // fallback safety

		resolutionDropdown.SetValueWithoutNotify(recommendedIndex);
		displayModeDropdown.SetValueWithoutNotify(1);

		ApplySettings();
	}
}