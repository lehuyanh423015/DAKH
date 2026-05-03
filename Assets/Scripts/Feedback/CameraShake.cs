using UnityEngine;

/// <summary>
/// CameraShake – Simple procedural camera shake via position offset.
///
/// Usage:
///   Attach to the Main Camera.
///   Call Shake(duration, strength) from any script that has a reference.
///
/// How it works:
///   A coroutine offsets the camera's local position by a random amount each
///   frame, then restores the original position when the timer expires.
///   Multiple overlapping calls are handled gracefully: each new call
///   stops the previous coroutine and starts fresh.
///
/// Inspector recommended values for PlayerCombat:
///   miss shake  : duration 0.15, strength 0.08
///   game-over   : duration 0.25, strength 0.15
/// </summary>
public class CameraShake : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private Coroutine shakeCoroutine;

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Shakes the camera for <paramref name="duration"/> seconds,
    /// displacing it up to <paramref name="strength"/> units from its resting position.
    /// Safe to call while a shake is already running – the previous shake is cancelled.
    /// </summary>
    public void Shake(float duration, float strength)
    {
        // Cancel any in-progress shake before starting a new one.
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, strength));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Shake coroutine
    // ──────────────────────────────────────────────────────────────────────────

    private System.Collections.IEnumerator ShakeRoutine(float duration, float strength)
    {
        // Remember the original local position so we can restore it.
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Pick a random direction each frame and apply the offset.
            float offsetX = Random.Range(-1f, 1f) * strength;
            float offsetY = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);

            elapsed += Time.deltaTime;
            yield return null; // wait one frame
        }

        // Restore the camera to its original resting position.
        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}
