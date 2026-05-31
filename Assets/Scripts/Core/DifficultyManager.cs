using UnityEngine;

/// <summary>
/// DifficultyManager – Time-based difficulty scaling.
///
/// Responsibilities:
///   - Tracks elapsed gameplay time (paused when game is over).
///   - Calculates a difficulty multiplier from elapsed time.
///   - Exposes CurrentEnemySpeed and CurrentSpawnInterval for EnemySpawner.
///   - Optionally logs difficulty stats at a fixed interval.
///
/// Formula (beginner-friendly, easy to tune):
///   difficultyMultiplier  = 1 + elapsedTime * difficultyIncreaseRate
///   CurrentEnemySpeed     = Clamp(baseEnemySpeed  * multiplier, baseEnemySpeed, maxEnemySpeed)
///   CurrentSpawnInterval  = Clamp(baseSpawnInterval / multiplier, minSpawnInterval, baseSpawnInterval)
///
/// Scene setup:
///   1. Create an empty GameObject named "DifficultyManager".
///   2. Attach this script to it.
///   3. Drag the DifficultyManager component into EnemySpawner's "Difficulty Manager" slot.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Tunable difficulty parameters
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Enemy Speed")]
    [Tooltip("Starting enemy move speed (matches Enemy prefab default). Recommended: 2.5.")]
    [SerializeField] private float baseEnemySpeed = 2.5f;

    [Tooltip("Maximum enemy move speed the game will ever allow. Recommended: 6.0.")]
    [SerializeField] private float maxEnemySpeed = 6.0f;

    [Header("Spawn Interval")]
    [Tooltip("Starting seconds between enemy spawns (matches EnemySpawner default). Recommended: 1.5.")]
    [SerializeField] private float baseSpawnInterval = 1.5f;

    [Tooltip("Minimum seconds between enemy spawns — prevents impossibly fast spawning. Recommended: 0.45.")]
    [SerializeField] private float minSpawnInterval = 0.45f;

    [Header("Scaling Rate")]
    [Tooltip("How quickly difficulty increases per second of gameplay. " +
             "Higher = faster ramp. Recommended: 0.05 (5% per second).")]
    [SerializeField] private float difficultyIncreaseRate = 0.05f;

    [Header("Demo Mode (Phase 25)")]
    [Tooltip("If true and GameManager is in Demo Mode, override scaling with fixed values.")]
    [SerializeField] private bool useDemoDifficultyOverride = true;
    
    [Tooltip("Fixed enemy speed for Demo Mode.")]
    [SerializeField] private float demoEnemySpeed = 2.2f;
    
    [Tooltip("Fixed spawn interval for Demo Mode.")]
    [SerializeField] private float demoSpawnInterval = 1.4f;

    [Header("Debug Logging")]
    [Tooltip("Enable periodic difficulty logging to the Console.")]
    [SerializeField] private bool logDifficulty = true;

    [Tooltip("How often (seconds) to log difficulty info when logDifficulty is true.")]
    [SerializeField] private float logInterval = 10f;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private float elapsedTime    = 0f;
    private float logTimer       = 0f;

    // ──────────────────────────────────────────────────────────────────────────
    // Public read-only properties
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Current difficulty multiplier (grows linearly with elapsed time).</summary>
    public float DifficultyMultiplier => 1f + elapsedTime * difficultyIncreaseRate;

    /// <summary>
    /// Move speed enemies should use right now.
    /// Increases from baseEnemySpeed up to maxEnemySpeed.
    /// </summary>
    public float CurrentEnemySpeed
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.IsDemoMode && useDemoDifficultyOverride)
                return demoEnemySpeed;
                
            return Mathf.Clamp(baseEnemySpeed * DifficultyMultiplier, baseEnemySpeed, maxEnemySpeed);
        }
    }

    /// <summary>
    /// Spawn interval EnemySpawner should use right now.
    /// Decreases from baseSpawnInterval down to minSpawnInterval.
    /// </summary>
    public float CurrentSpawnInterval
    {
        get
        {
            if (GameManager.Instance != null && GameManager.Instance.IsDemoMode && useDemoDifficultyOverride)
                return demoSpawnInterval;
                
            return Mathf.Clamp(baseSpawnInterval / DifficultyMultiplier, minSpawnInterval, baseSpawnInterval);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Stop scaling when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        elapsedTime += Time.deltaTime;

        // Optional periodic debug log.
        if (logDifficulty)
        {
            logTimer += Time.deltaTime;
            if (logTimer >= logInterval)
            {
                logTimer = 0f;
                Debug.Log($"Difficulty: multiplier={DifficultyMultiplier:F2}, " +
                          $"enemySpeed={CurrentEnemySpeed:F2}, " +
                          $"spawnInterval={CurrentSpawnInterval:F2}");
            }
        }
    }
}
