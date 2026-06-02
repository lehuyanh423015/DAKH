using UnityEngine;

/// <summary>
/// Deprecated helper for old automatic menu layout.
/// Runtime layout is disabled by default because MainMenu panels are now positioned manually.
/// </summary>
public class MenuUIAutoLayout : MonoBehaviour
{
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform settingsPanel;
    [SerializeField] private bool applyLayoutAtRuntime = false;

    private void Start()
    {
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
    }

    private void ApplySettingsPanelLayout()
    {
        if (settingsPanel == null) return;
    }
}
