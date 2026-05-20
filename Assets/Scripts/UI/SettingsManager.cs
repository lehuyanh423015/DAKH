using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SettingsManager – Handles fullscreen, resolution, and master volume settings (Phase 23).
/// Settings are persisted using PlayerPrefs and applied on startup.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("UI References (optional)")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider masterVolumeSlider;

    [Header("Debug")]
    [SerializeField] private bool logSettings = true;

    // ──────────────────────────────────────────────────────────────────────────
    // Available resolutions
    // ──────────────────────────────────────────────────────────────────────────

    private static readonly (int width, int height)[] AvailableResolutions =
    {
        (1280, 720),
        (1600, 900),
        (1920, 1080),
    };

    // ──────────────────────────────────────────────────────────────────────────
    // PlayerPrefs keys
    // ──────────────────────────────────────────────────────────────────────────

    private const string KeyFullscreen      = "Fullscreen";
    private const string KeyResolutionIndex = "ResolutionIndex";
    private const string KeyMasterVolume    = "MasterVolume";

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime state
    // ──────────────────────────────────────────────────────────────────────────

    private bool  currentFullscreen      = false;
    private int   currentResolutionIndex = 0;
    private float currentMasterVolume    = 1.0f;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        PopulateResolutionDropdown();
        LoadSettings();
        ApplySettings();
        UpdateUI();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Dropdown setup
    // ──────────────────────────────────────────────────────────────────────────

    private void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null)
        {
            if (logSettings) Debug.LogWarning("SettingsManager: ResolutionDropdown is not assigned.");
            return;
        }

        resolutionDropdown.ClearOptions();
        var options = new System.Collections.Generic.List<string>();
        foreach (var (w, h) in AvailableResolutions)
        {
            options.Add($"{w} x {h}");
        }
        resolutionDropdown.AddOptions(options);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Load / Save / Apply
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Loads settings from PlayerPrefs into local state.</summary>
    public void LoadSettings()
    {
        currentFullscreen      = PlayerPrefs.GetInt(KeyFullscreen, 0) == 1;
        currentResolutionIndex = PlayerPrefs.GetInt(KeyResolutionIndex, 0);
        currentMasterVolume    = PlayerPrefs.GetFloat(KeyMasterVolume, 1.0f);

        // Clamp index in case resolution list has changed.
        currentResolutionIndex = Mathf.Clamp(currentResolutionIndex, 0, AvailableResolutions.Length - 1);

        if (logSettings)
        {
            var (w, h) = AvailableResolutions[currentResolutionIndex];
            Debug.Log($"SettingsManager: Loaded – Fullscreen={currentFullscreen}, " +
                      $"Resolution={w}x{h}, Volume={currentMasterVolume:0.00}");
        }
    }

    /// <summary>Applies current local state to the engine.</summary>
    public void ApplySettings()
    {
        // Resolution & fullscreen
        var (width, height) = AvailableResolutions[currentResolutionIndex];
        Screen.SetResolution(width, height, currentFullscreen);

        // Master volume
        AudioListener.volume = currentMasterVolume;

        if (logSettings)
        {
            Debug.Log($"SettingsManager: Applied – {width}x{height}, " +
                      $"Fullscreen={currentFullscreen}, Volume={currentMasterVolume:0.00}");
        }
    }

    /// <summary>Saves current local state to PlayerPrefs.</summary>
    public void SaveSettings()
    {
        PlayerPrefs.SetInt(KeyFullscreen, currentFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(KeyResolutionIndex, currentResolutionIndex);
        PlayerPrefs.SetFloat(KeyMasterVolume, currentMasterVolume);
        PlayerPrefs.Save();

        if (logSettings) Debug.Log("SettingsManager: Settings saved.");
    }

    /// <summary>Pushes current local state into the UI controls.</summary>
    private void UpdateUI()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(currentFullscreen);
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.SetValueWithoutNotify(currentResolutionIndex);
            resolutionDropdown.RefreshShownValue();
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(currentMasterVolume);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // UI callbacks – wire these up in the Inspector
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Called by FullscreenToggle.OnValueChanged.</summary>
    public void OnFullscreenChanged(bool isFullscreen)
    {
        currentFullscreen = isFullscreen;
        ApplySettings();
        SaveSettings();
    }

    /// <summary>Called by ResolutionDropdown.OnValueChanged.</summary>
    public void OnResolutionChanged(int index)
    {
        currentResolutionIndex = Mathf.Clamp(index, 0, AvailableResolutions.Length - 1);
        ApplySettings();
        SaveSettings();
    }

    /// <summary>Called by MasterVolumeSlider.OnValueChanged.</summary>
    public void OnMasterVolumeChanged(float volume)
    {
        currentMasterVolume = volume;
        AudioListener.volume = volume;
        // Save only when interaction ends to avoid spamming PlayerPrefs.
    }

    /// <summary>Called when the slider interaction ends (e.g., pointer up). Saves volume.</summary>
    public void OnMasterVolumeReleased()
    {
        SaveSettings();
    }
}
