using System.Collections;
using UnityEngine;

/// <summary>
/// UIPopEffect – Gives a UI element a quick scale "pop" when important values change.
/// </summary>
public class UIPopEffect : MonoBehaviour
{
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.12f;
    [SerializeField] private bool logPop = false;

    private Vector3 initialScale;
    private Coroutine popCoroutine;

    private void Awake()
    {
        initialScale = transform.localScale;
    }

    public void PlayPop()
    {
        if (!gameObject.activeInHierarchy) return;

        if (logPop) Debug.Log($"[{gameObject.name}] Playing Pop Effect");

        if (popCoroutine != null)
        {
            StopCoroutine(popCoroutine);
        }
        popCoroutine = StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        transform.localScale = initialScale * popScale;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);
            transform.localScale = Vector3.Lerp(initialScale * popScale, initialScale, t);
            yield return null;
        }

        transform.localScale = initialScale;
    }
}
