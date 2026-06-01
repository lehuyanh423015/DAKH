using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies the final left-side MainMenu layout while keeping the gameplay demo visible.
/// </summary>
public class FinalMenuLayoutApplier : MonoBehaviour
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
        mainPanel.sizeDelta = new Vector2(420f, 560f);
        mainPanel.anchoredPosition = new Vector2(260f, 0f);

        Image panelImage = mainPanel.GetComponent<Image>();
        if (panelImage != null)
        {
            Color color = panelImage.color;
            color.a = 0f;
            panelImage.color = color;
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

        settingsPanel.anchorMin = new Vector2(0.5f, 0.5f);
        settingsPanel.anchorMax = new Vector2(0.5f, 0.5f);
        settingsPanel.pivot = new Vector2(0.5f, 0.5f);
        settingsPanel.sizeDelta = new Vector2(620f, 620f);
        settingsPanel.anchoredPosition = Vector2.zero;
    }
}
