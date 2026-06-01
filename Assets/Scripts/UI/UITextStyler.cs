using TMPro;
using UnityEngine;

/// <summary>
/// Applies consistent cyber/night text color, outline, and optional glow to TMP text.
/// </summary>
public class UITextStyler : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Color outlineColor = new Color(0f, 0.85f, 1f, 1f);
    [SerializeField] private float outlineWidth = 0.18f;
    [SerializeField] private bool useGlowColor = true;
    [SerializeField] private Color glowColor = new Color(0f, 0.85f, 1f, 0.55f);
    [SerializeField] private int fontSize = 48;
    [SerializeField] private bool applyOnStart = true;

    private void Awake()
    {
        AutoDetectText();
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
        AutoDetectText();
        if (text == null) return;

        text.color = textColor;
        text.fontSize = fontSize;
        text.outlineColor = outlineColor;
        text.outlineWidth = outlineWidth;

        if (useGlowColor && text.fontMaterial != null)
        {
            Material material = new Material(text.fontMaterial);

            if (material.HasProperty("_GlowColor"))
            {
                material.EnableKeyword("GLOW_ON");
                material.SetColor("_GlowColor", glowColor);
            }

            text.fontMaterial = material;
        }
    }

    private void AutoDetectText()
    {
        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }
    }
}
