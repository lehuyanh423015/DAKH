using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// UIFlashEffect – Flashes UI text color briefly.
/// </summary>
public class UIFlashEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.18f;
    [SerializeField] private bool logFlash = false;

    private Color originalColor;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TextMeshProUGUI>();
        }

        if (targetText != null)
        {
            originalColor = targetText.color;
        }
    }

    public void PlayFlash()
    {
        if (targetText == null || !gameObject.activeInHierarchy) return;

        if (logFlash) Debug.Log($"[{gameObject.name}] Playing Flash Effect");

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        targetText.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            targetText.color = Color.Lerp(flashColor, originalColor, t);
            yield return null;
        }

        targetText.color = originalColor;
    }
}
