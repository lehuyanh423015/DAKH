using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerMovement – Handles small attack-momentum shifts for the player.
///
/// Design goal (Phase 10):
///   The player has no free WASD movement. Instead, after every accepted attack input,
///   the player drifts slightly in the attack direction. This adds game feel without
///   turning the player into a mobile character.
///
/// Responsibilities:
///   - Smoothly lerp the player to a new X position after a shift is requested.
///   - Clamp the final position within [minX, maxX] so the player stays in the combat zone.
///   - Expose ShiftLeft() / ShiftRight() / ShiftByDirection() for PlayerCombat to call.
///   - If a shift is already in progress, cancel it and start fresh from the current position.
///   - Never read input directly — input is handled by PlayerCombat.
///
/// Inspector recommended values:
///   shiftDistance : 0.25  (tune between 0.15–0.35)
///   shiftDuration : 0.08  (tune between 0.05–0.12)
///   minX          : -2.5
///   maxX          :  2.5
///   logMovement   : false
///
/// Attach to: the Player GameObject, alongside PlayerCombat.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Shift Settings")]
    [SerializeField]
    [Tooltip("World-space units the player shifts per attack. " +
             "Small values feel subtle; larger values feel punchy. " +
             "Recommended: 0.25. Tune between 0.15–0.35.")]
    private float shiftDistance = 0.25f;

    [SerializeField]
    [Tooltip("Seconds the shift movement takes. " +
             "Shorter = snappier; longer = smoother. " +
             "Recommended: 0.08. Tune between 0.05–0.12.")]
    private float shiftDuration = 0.08f;

    [Header("Combat Zone")]
    [SerializeField]
    [Tooltip("Left boundary of the player's allowed X range. " +
             "Player cannot shift further left than this. Recommended: -2.5.")]
    private float minX = -2.5f;

    [SerializeField]
    [Tooltip("Right boundary of the player's allowed X range. " +
             "Player cannot shift further right than this. Recommended: 2.5.")]
    private float maxX = 2.5f;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log shift direction to the Console when a shift occurs.")]
    private bool logMovement = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private Coroutine shiftRoutine;

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Shift the player one step to the left (negative X).</summary>
    public void ShiftLeft()
    {
        ShiftByDirection(-1);
    }

    /// <summary>Shift the player one step to the right (positive X).</summary>
    public void ShiftRight()
    {
        ShiftByDirection(1);
    }

    /// <summary>
    /// Shifts the player in the given direction.
    ///   direction &lt; 0 → shift left.
    ///   direction &gt; 0 → shift right.
    ///   direction = 0 → no movement.
    ///
    /// Cancels any in-progress shift and starts fresh from the current position
    /// so rapid alternating attacks feel responsive.
    /// </summary>
    public void ShiftByDirection(int direction)
    {
        if (direction == 0) return;

        // Calculate the clamped target X.
        float currentX = transform.position.x;
        float rawTargetX  = currentX + direction * shiftDistance;
        float clampedTargetX = Mathf.Clamp(rawTargetX, minX, maxX);

        // Skip if already at the boundary (no visible movement).
        if (Mathf.Approximately(clampedTargetX, currentX)) return;

        if (logMovement)
        {
            Debug.Log(direction < 0 ? "Player shifted left." : "Player shifted right.");
        }

        // Cancel any running shift before starting a new one.
        if (shiftRoutine != null)
        {
            StopCoroutine(shiftRoutine);
        }
        shiftRoutine = StartCoroutine(ShiftRoutine(clampedTargetX));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Shift coroutine
    // ──────────────────────────────────────────────────────────────────────────

    private IEnumerator ShiftRoutine(float targetX)
    {
        Vector3 startPos  = transform.position;
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);

        float elapsed = 0f;

        while (elapsed < shiftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shiftDuration);

            // Smooth-step for a slightly more organic feel than linear lerp.
            float smoothT = t * t * (3f - 2f * t);
            transform.position = Vector3.Lerp(startPos, targetPos, smoothT);

            yield return null;
        }

        // Snap to target and re-clamp to guarantee boundary precision.
        Vector3 final = transform.position;
        final.x = Mathf.Clamp(targetX, minX, maxX);
        transform.position = final;

        shiftRoutine = null;
    }
}
