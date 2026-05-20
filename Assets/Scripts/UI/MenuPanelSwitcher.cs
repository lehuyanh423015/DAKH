using UnityEngine;

/// <summary>
/// MenuPanelSwitcher – Toggles between Main Panel and Settings Panel in the Main Menu (Phase 23).
/// </summary>
public class MenuPanelSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    private void Start()
    {
        // Ensure the main panel is active and settings panel is hidden on start.
        ShowMain();
    }

    /// <summary>Shows the Main Panel and hides the Settings Panel.</summary>
    public void ShowMain()
    {
        if (mainPanel != null)    mainPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    /// <summary>Shows the Settings Panel and hides the Main Panel.</summary>
    public void ShowSettings()
    {
        if (mainPanel != null)    mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }
}
