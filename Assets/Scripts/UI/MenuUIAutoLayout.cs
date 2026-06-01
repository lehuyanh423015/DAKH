using UnityEngine;

/// <summary>
/// Applies a quick, consistent layout to main menu panels.
/// This only moves/resizes panels and does not reorder child controls.
/// </summary>
public class MenuUIAutoLayout : MonoBehaviour
{
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform settingsPanel;
    [SerializeField] private bool applyOnStart = true;

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyLayout();
        }
    }

    public void ApplyLayout()
    {
        ApplyMainPanelLayout();
        ApplySettingsPanelLayout();
    }

    private void ApplyMainPanelLayout()
    {
        if (mainPanel == null) return;

        mainPanel.anchorMin = new Vector2(0f, 0.5f);
        mainPanel.anchorMax = new Vector2(0f, 0.5f);
        mainPanel.pivot = new Vector2(0.5f, 0.5f);
        mainPanel.sizeDelta = new Vector2(460f, 520f);
        mainPanel.anchoredPosition = new Vector2(320f, 0f);
    }

    private void ApplySettingsPanelLayout()
    {
        if (settingsPanel == null) return;

        settingsPanel.anchorMin = new Vector2(0.5f, 0.5f);
        settingsPanel.anchorMax = new Vector2(0.5f, 0.5f);
        settingsPanel.pivot = new Vector2(0.5f, 0.5f);
        settingsPanel.sizeDelta = new Vector2(560f, 620f);
        settingsPanel.anchoredPosition = Vector2.zero;
    }
}
