using UnityEngine;

/// <summary>
/// CameraFollow – Smoothly follows the player's X position.
///
/// Phase 11 additions:
///   - LateUpdate-based X tracking so player movement settles first.
///   - followFactor scales how much of the player's X offset is reflected on the camera.
///     A value of 1.0 means the camera centres on the player; 0.6 means only 60% offset.
///   - Dead zone: camera ignores very small deviations to prevent jitter from tiny shifts.
///   - Optional X bounds keep the camera from drifting off the fixed backdrop.
///
/// CameraShake compatibility:
///   CameraShake applies random offsets around the camera position captured at shake-start.
///   Because both scripts operate on the same Transform, they can briefly conflict on the
///   frame the shake ends (CameraShake restores its snapshot; CameraFollow overrides next
///   LateUpdate). In practice this is a single frame and unnoticeable at normal frame rates.
///   No camera rig or additional GameObjects are required.
///
/// Inspector recommended values:
///   followStrength : 3.0   (higher = snappier follow)
///   followFactor   : 0.6   (0 = no follow, 1 = full centre on player)
///   deadZone       : 0.4   (units of deviation ignored)
///   useBounds      : true
///   minX           : -1.5
///   maxX           :  1.5
///
/// Attach to: Main Camera (alongside CameraShake if shake is used).
/// Set target: Player transform.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Target")]
    [SerializeField]
    [Tooltip("The Transform this camera follows on the X axis. Drag the Player here.")]
    private Transform target;

    [Header("Follow Settings")]
    [SerializeField]
    [Tooltip("How quickly the camera catches up to the desired position. " +
             "Higher = snappier. Recommended: 3.0.")]
    private float followStrength = 3.0f;

    [SerializeField]
    [Tooltip("How much of the player's X offset from the initial camera position " +
             "is reflected on the camera. " +
             "0 = camera never moves, 1 = camera fully centres on player. " +
             "Recommended: 0.6 for subtle follow.")]
    private float followFactor = 0.6f;

    [SerializeField]
    [Tooltip("Minimum deviation (world units) before the camera starts following. " +
             "Prevents jitter from tiny player shifts. Recommended: 0.4.")]
    private float deadZone = 0.4f;

    [Header("Bounds")]
    [SerializeField]
    [Tooltip("Clamp camera X position within [minX, maxX] when enabled.")]
    private bool useBounds = true;

    [SerializeField]
    [Tooltip("Left limit of camera X movement. Recommended: -1.5.")]
    private float minX = -1.5f;

    [SerializeField]
    [Tooltip("Right limit of camera X movement. Recommended: 1.5.")]
    private float maxX = 1.5f;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log camera follow events. Keep false in production.")]
    private bool logCameraFollow = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>World position of the camera at scene start.</summary>
    private Vector3 initialPosition;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        initialPosition = transform.position;
    }

    /// <summary>
    /// LateUpdate ensures the player has already moved this frame before we
    /// read its position, giving smooth one-frame-delayed tracking.
    /// </summary>
    private void LateUpdate()
    {
        if (target == null) return;

        // Phase 24: Stop camera follow on game over or when paused.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (Time.timeScale == 0f) return;

        Vector3 current = transform.position;

        // Desired X = camera's initial X + a fraction of the player's current X.
        // followFactor < 1 means the camera does not fully centre on the player.
        float desiredX = initialPosition.x + target.position.x * followFactor;

        // Apply optional X bounds before the dead-zone check.
        if (useBounds)
        {
            desiredX = Mathf.Clamp(desiredX, minX, maxX);
        }

        // Dead zone: skip movement if the deviation is small enough.
        float deviation = Mathf.Abs(desiredX - current.x);
        if (deviation <= deadZone)
        {
            return;
        }

        // Smooth lerp toward desired X; preserve the initial Y and current Z.
        // Using current.z (not initialPosition.z) keeps it compatible with
        // CameraShake which may have moved Z or is running on the same frame.
        float newX = Mathf.Lerp(current.x, desiredX, followStrength * Time.deltaTime);
        transform.position = new Vector3(newX, initialPosition.y, current.z);

        if (logCameraFollow)
        {
            Debug.Log($"CameraFollow: player={target.position.x:F2}  desiredX={desiredX:F2}  camX={newX:F2}");
        }
    }
}
