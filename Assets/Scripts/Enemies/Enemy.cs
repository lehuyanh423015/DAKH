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
/// Phase 8 tuning (preserved):
///   - knockbackDistance default reduced: 1.0 → 0.45
///   - knockbackDuration default reduced: 0.12 → 0.08
///
/// Phase 9 additions:
///   - Lane blocking: a back-of-queue enemy pauses if the enemy ahead is too close.
///   - IsBlockedByFrontEnemy() scans same-side enemies each frame and checks spacing.
///   - enableLaneBlocking and minSpacingFromFrontEnemy are Inspector-tunable.
///   - logLaneBlocking prints a one-shot debug message when blocking is active.
///   - Works with knockback: a knocked-back front enemy still blocks those behind it.
///   - No deadlock: once the front enemy is destroyed, this enemy resumes automatically.
///
/// Design note:
///   Enemy types should NOT differ by movement speed.
///   Speed is controlled globally by DifficultyManager.
///   Difficulty comes from hit count, knockback, and lane pressure.
///
/// Collider setup (required for OnTriggerEnter2D to fire):
///   - Player Box Collider 2D → Is Trigger = TRUE
///   - Enemy Box Collider 2D  → Is Trigger = FALSE  (solid collider)
///   - Enemy Rigidbody 2D     → Body Type  = Kinematic
/// </summary>
public class Enemy : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Spawn-side enum
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Which horizontal side this enemy spawned from.</summary>
    public enum SpawnSide { Left, Right }

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Movement & score  (Phase 1/2/5)
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
    // Inspector fields – Health & knockback  (Phase 7/8)
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

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Lane blocking  (Phase 9)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Lane Blocking (Phase 9)")]
    [SerializeField]
    [Tooltip("Enable same-side queue behaviour: this enemy pauses when the enemy ahead " +
             "is closer than minSpacingFromFrontEnemy. Prevents unfair overlapping and " +
             "enemies slipping past a HeavyEnemy that is blocking the lane.")]
    private bool enableLaneBlocking = true;

    [SerializeField]
    [Tooltip("Minimum world-space distance this enemy keeps from the same-side enemy " +
             "ahead of it. If the gap drops below this value, this enemy stops for that frame. " +
             "Recommended: 0.8. Tune to match enemy scale.")]
    private float minSpacingFromFrontEnemy = 0.8f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Debug
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log hit info to the Console for this enemy type.")]
    private bool logEnemyHits = false;

    [SerializeField]
    [Tooltip("Log a message when lane blocking is active. " +
             "Keep false in production to avoid Console spam.")]
    private bool logLaneBlocking = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime data
    // ──────────────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private int       currentHealth;
    private bool      isKnockedBack    = false;

    // Throttle the lane-blocking log so it doesn't fire every frame.
    private bool      wasBlockedLastFrame = false;

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

        // ── Phase 9: lane blocking check ─────────────────────────────────────
        if (enableLaneBlocking && IsBlockedByFrontEnemy())
        {
            // Log only on the frame blocking starts (not every blocked frame).
            if (logLaneBlocking && !wasBlockedLastFrame)
            {
                Debug.Log($"[{gameObject.name}] Lane blocked by front enemy.");
            }
            wasBlockedLastFrame = true;
            return; // skip movement this frame
        }
        wasBlockedLastFrame = false;

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
    // Lane blocking helpers  (Phase 9)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if there is a same-side enemy ahead of this one that is
    /// closer than minSpacingFromFrontEnemy, meaning this enemy should pause.
    ///
    /// "Ahead" means closer to the player along the X axis:
    ///   Left-side enemy  → higher X is closer to the (right-positioned) player.
    ///   Right-side enemy → lower  X is closer to the (left-positioned) player.
    /// </summary>
    private bool IsBlockedByFrontEnemy()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        float myX            = transform.position.x;
        float closestGap     = float.MaxValue;
        bool  foundFrontEnemy = false;

        foreach (Enemy other in allEnemies)
        {
            // Skip self.
            if (other == this) continue;

            // Skip enemies on the opposite side.
            if (other.Side != Side) continue;

            // Skip defeated enemies (destroyed ones won't be in the array).
            if (other.IsDefeated) continue;

            float otherX = other.transform.position.x;

            // Check if 'other' is in front of (closer to the player than) this enemy.
            if (!IsSameSideEnemyInFront(otherX, myX)) continue;

            // Measure gap.
            float gap = DistanceTo(other);
            if (gap < closestGap)
            {
                closestGap     = gap;
                foundFrontEnemy = true;
            }
        }

        // Block if the nearest front enemy is within the spacing threshold.
        return foundFrontEnemy && closestGap < minSpacingFromFrontEnemy;
    }

    /// <summary>
    /// Returns true if an enemy at otherX is "in front" of this enemy at myX,
    /// i.e. it is closer to the player along the movement axis.
    ///
    /// Left-side enemies travel right (+X) → front = higher X than myX.
    /// Right-side enemies travel left (−X) → front = lower  X than myX.
    /// </summary>
    private bool IsSameSideEnemyInFront(float otherX, float myX)
    {
        if (Side == SpawnSide.Left)
        {
            return otherX > myX;  // other is closer to the right-side player
        }
        else
        {
            return otherX < myX;  // other is closer to the left-side player
        }
    }

    /// <summary>
    /// Returns the 2D distance between this enemy and another.
    /// Uses only XY position (ignores Z).
    /// </summary>
    private float DistanceTo(Enemy other)
    {
        return Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(other.transform.position.x, other.transform.position.y));
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
    // Public API – Phase 7 (preserved)
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
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        if (IsDefeated) return false;

        currentHealth--;

        if (logEnemyHits)
        {
            Debug.Log($"[{gameObject.name}] Hit! HP: {currentHealth}/{maxHealth}");
        }

        if (currentHealth <= 0)
        {
            return true; // Defeated — caller handles effect + destroy.
        }
        else
        {
            TriggerKnockback();
            return false;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Knockback  (Phase 7, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the knockback coroutine.
    /// Direction: away from the player along the spawn axis.
    ///   Left-side enemy  → knocked further left  (−X).
    ///   Right-side enemy → knocked further right (+X).
    ///
    /// Note: while this enemy is knocked back, it still counts as a "front enemy"
    /// for blocking purposes, so enemies behind it correctly pause during the knockback.
    /// </summary>
    private void TriggerKnockback()
    {
        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        float directionX         = (Side == SpawnSide.Left) ? -1f : 1f;
        Vector3 knockbackOffset  = new Vector3(directionX * knockbackDistance, 0f, 0f);

        if (knockbackDuration <= 0f)
        {
            transform.position += knockbackOffset;
        }
        else
        {
            Vector3 startPos  = transform.position;
            Vector3 targetPos = transform.position + knockbackOffset;
            float   elapsed   = 0f;

            while (elapsed < knockbackDuration)
            {
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
        // Normal movement toward player resumes in Update().
        // Enemies behind this one resume as soon as the gap grows beyond minSpacingFromFrontEnemy.
    }
}
