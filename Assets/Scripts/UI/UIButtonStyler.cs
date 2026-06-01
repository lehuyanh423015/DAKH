using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a consistent visual style to UI buttons without changing their click events.
/// </summary>
public class UIButtonStyler : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Color normalColor = new Color(5f / 255f, 18f / 255f, 32f / 255f, 0.72f);
    [SerializeField] private Color highlightedColor = new Color(0f, 200f / 255f, 255f / 255f, 0.95f);
    [SerializeField] private Color pressedColor = new Color(1f, 180f / 255f, 40f / 255f, 1f);
    [SerializeField] private Color selectedColor = new Color(0f, 220f / 255f, 255f / 255f, 1f);
    [SerializeField] private Color disabledColor = new Color(60f / 255f, 60f / 255f, 60f / 255f, 0.45f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 32;
    [SerializeField] private bool applyOnStart = true;

    [Header("Optional Polish (Phase 26)")]
    [SerializeField] private bool addImageOutline = true;
    [SerializeField] private Color imageOutlineColor = new Color(0f, 1f, 1f, 1f);
    [SerializeField] private Vector2 imageOutlineDistance = new Vector2(2f, -2f);

    [SerializeField] private bool addTextShadow = true;
    [SerializeField] private Color textShadowColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Vector2 textShadowDistance = new Vector2(2f, -2f);

    private void Awake()
    {
        AutoDetectReferences();
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
        AutoDetectReferences();

        if (button != null)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = highlightedColor;
            colors.pressedColor = pressedColor;
            colors.selectedColor = selectedColor;
            colors.disabledColor = disabledColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        if (buttonImage != null)
        {
            buttonImage.color = normalColor;

            if (addImageOutline)
            {
                Outline outline = buttonImage.GetComponent<Outline>();
                if (outline == null) outline = buttonImage.gameObject.AddComponent<Outline>();
                outline.effectColor = imageOutlineColor;
                outline.effectDistance = imageOutlineDistance;
            }
        }

        if (buttonText != null)
        {
            buttonText.color = textColor;
            buttonText.fontSize = fontSize;

            if (addTextShadow)
            {
                Shadow shadow = buttonText.GetComponent<Shadow>();
                if (shadow == null) shadow = buttonText.gameObject.AddComponent<Shadow>();
                shadow.effectColor = textShadowColor;
                shadow.effectDistance = textShadowDistance;
            }
        }
    }

    private void AutoDetectReferences()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (buttonImage == null)
        {
            buttonImage = GetComponent<Image>();
        }

        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }
}
