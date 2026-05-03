using System.Collections;
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
/// Phase 5 additions (preserved):
///   - SetMoveSpeed(float) : EnemySpawner overrides speed at spawn time from DifficultyManager.
///
/// Phase 7 additions (preserved):
///   - maxHealth / currentHealth : enemies can now survive more than one hit.
///   - TakeHit() : called by PlayerCombat; reduces health by 1, returns true if defeated.
///   - TriggerKnockback() : pushes a living enemy backward for knockbackDuration seconds.
///   - Movement pauses while isKnockedBack is true.
///   - logEnemyHits : optional per-prefab hit logging for debugging.
///
/// Phase 8 tuning:
///   - knockbackDistance default reduced: 1.0 → 0.45
///     (shorter push so HeavyEnemy stays reachable after the first hit)
///   - knockbackDuration default reduced: 0.12 → 0.08
///     (faster recovery maintains second-hit rhythm)
///
/// Design note (Phase 7):
///   Enemy types should NOT differ by movement speed.
///   Speed is controlled globally by DifficultyManager.
///   Enemies differ by maxHealth, scoreValue, visual appearance, and future patterns.
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
    // Inspector fields – Movement & score  (Phase 1/2/5, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Movement & Score")]
    [SerializeField]
    [Tooltip("Units per second the enemy moves toward the player. " +
             "Overridden at spawn time by EnemySpawner when DifficultyManager is assigned. " +
             "Keep equal across all enemy types — use maxHealth, not speed, to add difficulty. " +
             "Recommended: 2.5.")]
    private float moveSpeed = 2.5f;

    [SerializeField]
    [Tooltip("Base score awarded when this enemy is defeated. " +
             "NormalEnemy = 100. HeavyEnemy = 200.")]
    private int scoreValue = 100;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Health & knockback  (Phase 7)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Health (Phase 7)")]
    [SerializeField]
    [Tooltip("Total hits required to defeat this enemy. NormalEnemy = 1. HeavyEnemy = 2.")]
    private int maxHealth = 1;

    [Header("Knockback (Phase 7/8)")]
    [SerializeField]
    [Tooltip("World-space units the enemy is pushed back on a non-lethal hit. " +
             "Shorter values keep the enemy within comfortable second-hit range. " +
             "Recommended default: 0.45. Tune between 0.35–0.65 for feel.")]
    private float knockbackDistance = 0.45f;

    [SerializeField]
    [Tooltip("Seconds the knockback movement takes. " +
             "Shorter values let the enemy resume approach quickly so second hits feel natural. " +
             "Recommended default: 0.08. Tune between 0.06–0.12.")]
    private float knockbackDuration = 0.08f;

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log hit info to the Console for this enemy type.")]
    private bool logEnemyHits = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime data
    // ──────────────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private int       currentHealth;
    private bool      isKnockedBack = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Public properties
    // ──────────────────────────────────────────────────────────────────────────

    public SpawnSide Side          { get; private set; }
    public int       ScoreValue    => scoreValue;
    public int       CurrentHealth => currentHealth;
    public int       MaxHealth     => maxHealth;
    public bool      IsDefeated    => currentHealth <= 0;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Stop moving when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (playerTransform == null) return;

        // Pause normal movement while knocked back.
        if (isKnockedBack) return;

        // Move straight toward the player.
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API – Phase 5 (preserved)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by EnemySpawner immediately after Instantiate.
    /// Wires up the player reference and records which side this enemy spawned from.
    /// </summary>
    public void Initialize(Transform player, SpawnSide side)
    {
        playerTransform = player;
        Side            = side;
    }

    /// <summary>
    /// Overrides the enemy's move speed at spawn time.
    /// Called by EnemySpawner when DifficultyManager is assigned.
    /// All enemy types should receive the SAME global speed — do not use this
    /// to give HeavyEnemy a different (slower/faster) speed.
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API – Phase 7 (new)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by PlayerCombat when the player lands a valid hit on this enemy.
    /// Reduces currentHealth by 1.
    ///
    /// Returns true  → enemy is defeated (currentHealth reached 0).
    /// Returns false → enemy is still alive; knockback is triggered.
    ///
    /// Does nothing if the game is already over or the enemy is already defeated.
    /// </summary>
    public bool TakeHit()
    {
        // Guard: ignore hits after game over or after the enemy is already dead.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        if (IsDefeated) return false;

        currentHealth--;

        if (logEnemyHits)
        {
            Debug.Log($"[{gameObject.name}] Hit! HP: {currentHealth}/{maxHealth}");
        }

        if (currentHealth <= 0)
        {
            // Defeated — caller handles effect + destroy.
            return true;
        }
        else
        {
            // Still alive — push backward.
            TriggerKnockback();
            return false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Knockback  (Phase 7)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the knockback coroutine.
    /// Direction is determined by spawn side:
    ///   Left-side enemy → knocked further left  (negative X).
    ///   Right-side enemy → knocked further right (positive X).
    /// </summary>
    private void TriggerKnockback()
    {
        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        // Determine knockback direction: away from player (same as spawn side).
        float directionX = (Side == SpawnSide.Left) ? -1f : 1f;
        Vector3 knockbackOffset = new Vector3(directionX * knockbackDistance, 0f, 0f);

        if (knockbackDuration <= 0f)
        {
            // Instant position offset.
            transform.position += knockbackOffset;
        }
        else
        {
            // Smooth movement over knockbackDuration seconds.
            Vector3 startPos  = transform.position;
            Vector3 targetPos = transform.position + knockbackOffset;
            float   elapsed   = 0f;

            while (elapsed < knockbackDuration)
            {
                // Stop mid-knockback if game ends.
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                {
                    isKnockedBack = false;
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / knockbackDuration);
                transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
        }

        isKnockedBack = false;
        // Normal movement toward player resumes automatically in Update().
    }
}
