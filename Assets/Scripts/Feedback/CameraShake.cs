using UnityEngine;

/// <summary>
/// CameraShake – Simple procedural camera shake via position offset.
///
/// Usage:
///   Attach to the Main Camera.
///   Call Shake(duration, strength) from any script that has a reference.
///
/// How it works:
///   A coroutine offsets the camera's world position by a random amount each
///   frame, then restores the position captured at the moment Shake() was called.
///   Multiple overlapping calls are handled gracefully: each new call
///   stops the previous coroutine and starts fresh.
///
/// Phase 11 update (CameraFollow compatibility):
///   The base position is now sampled from transform.position at the START of
///   each ShakeRoutine call (not once at scene start). This ensures that when
///   CameraFollow has already moved the camera to a new X, the shake oscillates
///   around that followed position and restores to it — not the original scene origin.
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
    /// displacing it up to <paramref name="strength"/> units from its current position.
    /// Safe to call while a shake is already running – the previous shake is cancelled.
    /// </summary>
    public void Shake(float duration, float strength)
    {
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
        // Snapshot the current world position at the moment the shake begins.
        // If CameraFollow has moved the camera, this captures that followed position,
        // so the shake oscillates around it and restores to it correctly.
        Vector3 basePosition = transform.position;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                break;
            }

            if (Time.timeScale > 0f)
            {
                float offsetX = Random.Range(-1f, 1f) * strength;
                float offsetY = Random.Range(-1f, 1f) * strength;

                transform.position = basePosition + new Vector3(offsetX, offsetY, 0f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore to the base position captured at shake-start.
        // CameraFollow's next LateUpdate will then smoothly move it to the
        // correct followed position, so there is no visible snap.
        transform.position = basePosition;
        shakeCoroutine = null;
    }
}
