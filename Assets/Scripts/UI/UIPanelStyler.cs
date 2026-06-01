using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a consistent semi-transparent background style to UI panels.
/// Intended for menu, settings, pause, and game-over panels.
/// </summary>
public class UIPanelStyler : MonoBehaviour
{
    [SerializeField] private Image panelImage;
    [SerializeField] private Color panelColor = new Color(0.02f, 0.04f, 0.08f, 0.72f);
    [SerializeField] private bool applyOnStart = true;

    private void Awake()
    {
        EnsurePanelImage();
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplyStyle();
        }
    }

    public void ApplyStyle()
    {
        EnsurePanelImage();

        if (panelImage != null)
        {
            panelImage.color = panelColor;
        }
    }

    private void EnsurePanelImage()
    {
        if (panelImage == null)
        {
            panelImage = GetComponent<Image>();
        }

        if (panelImage == null)
        {
            panelImage = gameObject.AddComponent<Image>();
        }
    }
}
