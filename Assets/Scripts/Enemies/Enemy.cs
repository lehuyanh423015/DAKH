using UnityEngine;

/// <summary>
/// Enemy – Controls a single enemy instance.
///
/// Responsibilities:
///   - Moves toward the player every frame at a fixed speed.
///   - Knows which side it spawned from (Left or Right).
///   - Stops moving when the game is over.
///   - Triggers game over when it makes contact with the player.
///
/// Collider setup (important for OnTriggerEnter2D to fire):
///   - Player Box Collider 2D → Is Trigger = TRUE
///   - Enemy Box Collider 2D  → Is Trigger = FALSE  (solid collider)
///   - Enemy Rigidbody 2D     → Body Type  = Kinematic
///
///   In Unity's trigger rules, a trigger event fires when at least one of
///   the two colliders is a trigger AND at least one has a Rigidbody.
///   The kinematic Rigidbody on the enemy satisfies that requirement,
///   so OnTriggerEnter2D is called on the PLAYER's collider.
///   We detect the collision here on the enemy by tagging the player "Player"
///   and checking the tag inside OnTriggerEnter2D.
///
/// Scene / Inspector setup:
///   - Attach this script to the Enemy prefab in Assets/Prefabs.
///   - The spawner calls Initialize() after instantiating; no manual wiring needed.
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
    [Tooltip("Units per second the enemy moves toward the player. Recommended: 2–3.")]
    private float moveSpeed = 2.5f;

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime data  (set by EnemySpawner via Initialize)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>The player Transform this enemy chases.</summary>
    private Transform playerTransform;

    /// <summary>Which side this enemy came from (used by PlayerCombat).</summary>
    public SpawnSide Side { get; private set; }

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

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Stop moving if game is over or player reference is missing.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (playerTransform == null) return;

        // Move straight toward the player along the X axis (2D side-scroller style).
        // To move freely in both X and Y, replace with MoveTowards on both axes.
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only care about the player.
        // Make sure your Player GameObject has the tag "Player".
        if (!other.CompareTag("Player")) return;

        // Tell the GameManager the player was hit.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}
