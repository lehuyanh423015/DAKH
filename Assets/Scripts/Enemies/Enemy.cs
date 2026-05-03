using UnityEngine;

/// <summary>
/// Enemy – Controls a single enemy instance.
///
/// Phase 1 responsibilities (preserved):
///   - Moves toward the player every frame.
///   - Knows which side it spawned from (Left or Right).
///   - Stops moving when game is over.
///   - Triggers GameOver when it touches the player via OnTriggerEnter2D.
///
/// Phase 2 additions (preserved):
///   - scoreValue field and ScoreValue property for scoring.
///
/// Phase 5 additions:
///   - SetMoveSpeed(float) : lets EnemySpawner override move speed at spawn time
///     based on the current DifficultyManager value.
///
/// Collider setup (required for OnTriggerEnter2D to fire):
///   - Player Box Collider 2D → Is Trigger = TRUE
///   - Enemy Box Collider 2D  → Is Trigger = FALSE  (solid collider)
///   - Enemy Rigidbody 2D     → Body Type  = Kinematic
///
/// Scene / Inspector setup:
///   - Attach this script to the Enemy prefab in Assets/Prefabs.
///   - The spawner calls Initialize() and SetMoveSpeed() after instantiating.
/// </summary>
public class Enemy : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Spawn-side enum
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Which horizontal side this enemy spawned from.</summary>
    public enum SpawnSide { Left, Right }

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector-tunable fields
    // ──────────────────────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Units per second the enemy moves toward the player. " +
             "This is the DEFAULT value used when DifficultyManager is not assigned. " +
             "Recommended: 2.5. At runtime, EnemySpawner can override this via SetMoveSpeed().")]
    private float moveSpeed = 2.5f;

    [SerializeField]
    [Tooltip("Base score awarded when this enemy is destroyed. Recommended: 100.")]
    private int scoreValue = 100;

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime data  (set by EnemySpawner via Initialize / SetMoveSpeed)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>The player Transform this enemy chases.</summary>
    private Transform playerTransform;

    /// <summary>Which side this enemy came from (used by PlayerCombat for attack filtering).</summary>
    public SpawnSide Side { get; private set; }

    /// <summary>
    /// Base score this enemy is worth.
    /// PlayerCombat reads this before calling GameManager.RegisterHit(enemy.ScoreValue).
    /// </summary>
    public int ScoreValue => scoreValue;

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by EnemySpawner immediately after Instantiate.
    /// Wires up the player reference and records which side we spawned from.
    /// </summary>
    public void Initialize(Transform player, SpawnSide side)
    {
        playerTransform = player;
        Side = side;
    }

    /// <summary>
    /// Overrides the enemy's move speed at spawn time.
    /// Called by EnemySpawner when DifficultyManager is assigned,
    /// so each new enemy moves at the current difficulty-scaled speed.
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Stop moving if game is over or if the player reference is missing.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (playerTransform == null) return;

        // Move straight toward the player.
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only react to the Player.
        // Requires the Player GameObject to have the "Player" tag.
        if (!other.CompareTag("Player")) return;

        // Tell the GameManager the player was reached — triggers game over.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
