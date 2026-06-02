using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Deprecated helper for old automatic MainMenu layout.
/// Runtime layout is disabled by default because MainMenu panels are now positioned manually.
/// </summary>
public class FinalMenuLayoutApplier : MonoBehaviour
{
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform settingsPanel;
    [SerializeField] private bool applyLayoutAtRuntime = false;
    [SerializeField] private bool disableMainPanelBackgroundImage = true;

    private void Start()
    {
        DisableMainPanelBackgroundImageIfNeeded();

        if (applyLayoutAtRuntime)
        {
            ApplyLayout();
        }
    }

    public void ApplyLayout()
    {
        if (!applyLayoutAtRuntime)
        {
            return;
        }

        ApplyMainPanelLayout();
        ApplySettingsPanelLayout();
    }

    private void ApplyMainPanelLayout()
    {
        if (mainPanel == null) return;

        DisableMainPanelBackgroundImageIfNeeded();
    }

    private void DisableMainPanelBackgroundImageIfNeeded()
    {
        if (!disableMainPanelBackgroundImage || mainPanel == null)
        {
            return;
        }

        Image panelImage = mainPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.enabled = false;
        }

        UIPanelStyler panelStyler = mainPanel.GetComponent<UIPanelStyler>();
        if (panelStyler != null)
        {
            panelStyler.enabled = false;
        }
    }

    private void ApplySettingsPanelLayout()
    {
        if (settingsPanel == null) return;
    }
}
