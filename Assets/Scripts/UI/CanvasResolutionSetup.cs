using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies consistent CanvasScaler settings for the final 16:9 build target.
/// Add this to MainMenu and gameplay Canvas objects.
/// </summary>
public class CanvasResolutionSetup : MonoBehaviour
{
    [SerializeField] private bool applyOnStart = true;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    private void Awake()
    {
        if (!applyOnStart)
        {
            return;
        }

        ApplySettings();
    }

    private void Start()
    {
        if (applyOnStart)
        {
            ApplySettings();
        }
    }

    public void ApplySettings()
    {
        CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            return;
        }

        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = referenceResolution;
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = Mathf.Clamp01(matchWidthOrHeight);
    }
}
