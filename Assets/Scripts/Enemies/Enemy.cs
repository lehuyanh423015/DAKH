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
///   - knockbackDistance default 0.45, knockbackDuration default 0.08.
///
/// Phase 9 additions (preserved):
///   - Lane blocking: back-of-queue enemy pauses if front enemy is too close.
///   - ignoreLaneBlockingUntil: suppressed during side-switch crossing.
///
/// Phase 12 additions (preserved):
///   - EnemyBehaviorType enum: Normal | SwitchSideOnHit.
///   - SwitchEnemy: non-lethal hit → crosses to opposite side.
///
/// Phase 12 Hotfix v2 (this version):
///   - Landing distance is MIRRORED from player, not from enemy current X.
///   - Landing distance is CLAMPED between safeMinDistance and maxFollowupDistance.
///   - Queue-aware: respects enemies already on the destination side.
///   - isSwitchingSide blocks Game Over + extra hits during crossing.
///   - noGameOverUntil adds a brief post-landing grace period.
///   - Side enum is flipped BEFORE movement starts.
///   - New tuning fields: sideSwitchGap, rehitMargin, postSwitchNoGameOverDuration,
///     minSpacingOnTargetSide, targetFollowupMaxDistance.
///   - New helpers: GetHalfWidth, GetPlayerHalfWidth, GetCurrentCenterDistanceToPlayer,
///     CalculateSafeMinSwitchDistance, CalculateMaxFollowupDistance,
///     FindClosestFrontEnemyOnSide, CalculateQueueAwareSwitchTargetX.
///
/// Collider setup (required for OnTriggerEnter2D to fire):
///   - Player Box Collider 2D → Is Trigger = TRUE
///   - Enemy Box Collider 2D  → Is Trigger = FALSE  (solid collider)
///   - Enemy Rigidbody 2D     → Body Type  = Kinematic
/// </summary>
public class Enemy : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Enums
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Which horizontal side this enemy currently occupies.</summary>
    public enum SpawnSide { Left, Right }

    /// <summary>
    /// Controls the non-lethal-hit reaction.
    ///   Normal          → standard knockback.
    ///   SwitchSideOnHit → enemy crosses to the opposite side (SwitchEnemy).
    ///   AlternatingThreeHit → crosses to opposite side repeatedly (PatternEnemy3Hit).
    /// </summary>
    public enum EnemyBehaviorType { Normal, SwitchSideOnHit, AlternatingThreeHit }

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Movement & score  (Phase 1 / 2 / 5)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Movement & Score")]
    [SerializeField]
    [Tooltip("Units per second toward the player. Overridden at spawn time by EnemySpawner. " +
             "Keep equal across all enemy types. Recommended: 2.5.")]
    private float moveSpeed = 2.5f;

    [SerializeField]
    [Tooltip("Base score when defeated. NormalEnemy=100, HeavyEnemy=200, SwitchEnemy=250, PatternEnemy3Hit=350.")]
    private int scoreValue = 100;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Health & knockback  (Phase 7 / 8)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Health (Phase 7)")]
    [SerializeField]
    [Tooltip("Total hits to defeat. NormalEnemy=1. HeavyEnemy/SwitchEnemy=2. PatternEnemy3Hit=3.")]
    private int maxHealth = 1;

    [Header("Knockback (Phase 7/8 – Normal behavior only)")]
    [SerializeField]
    [Tooltip("Units pushed back on a non-lethal hit (Normal only). Recommended: 0.45.")]
    private float knockbackDistance = 0.45f;

    [SerializeField]
    [Tooltip("Seconds knockback takes (Normal only). Recommended: 0.08.")]
    private float knockbackDuration = 0.08f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Lane blocking  (Phase 9)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Lane Blocking (Phase 9)")]
    [SerializeField]
    [Tooltip("Pause when a same-side front enemy is closer than minSpacingFromFrontEnemy.")]
    private bool enableLaneBlocking = true;

    [SerializeField]
    [Tooltip("Minimum gap to the same-side front enemy. Recommended: 0.8.")]
    private float minSpacingFromFrontEnemy = 0.8f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Pattern behavior  (Phase 12)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Pattern Behavior (Phase 12 / 13)")]
    [SerializeField]
    [Tooltip("Non-lethal-hit reaction.\n" +
             "Normal          → knockback (NormalEnemy, HeavyEnemy).\n" +
             "SwitchSideOnHit → crosses to opposite side (SwitchEnemy).\n" +
             "AlternatingThreeHit → alternating sides (PatternEnemy3Hit).")]
    private EnemyBehaviorType behaviorType = EnemyBehaviorType.Normal;

    [SerializeField]
    [Tooltip("(Hotfix v2) Clear-air gap between enemy edge and player edge after landing.\n" +
             "safeMinDistance = sideSwitchGap + playerHalfWidth + enemyHalfWidth.\n" +
             "Recommended: 0.2 (small gap; main buffer comes from collider widths).")]
    private float sideSwitchGap = 0.2f;

    [SerializeField]
    [Tooltip("(Hotfix v2) Subtracted from attackRange to define the furthest attackable landing.\n" +
             "maxFollowupDistance = targetFollowupMaxDistance - rehitMargin.\n" +
             "Recommended: 0.15.")]
    private float rehitMargin = 0.15f;

    [SerializeField]
    [Tooltip("(Hotfix v2) Seconds after landing during which collision cannot trigger Game Over.\n" +
             "Prevents a single-frame overlap from instantly ending the game. Recommended: 0.06.")]
    private float postSwitchNoGameOverDuration = 0.06f;

    [SerializeField]
    [Tooltip("(Hotfix v2) Minimum gap between this enemy and the closest enemy already on the " +
             "destination side. Ensures SwitchEnemy does not land inside an existing queue. " +
             "Recommended: 0.7.")]
    private float minSpacingOnTargetSide = 0.7f;

    [SerializeField]
    [Tooltip("(Hotfix v2) Expected maximum attack range of the player. Used to keep the " +
             "landing position within follow-up range. Set this equal to PlayerCombat.attackRange. " +
             "Recommended: 2.0.")]
    private float targetFollowupMaxDistance = 2.0f;

    [SerializeField]
    [Tooltip("Seconds the side-switch crossing movement takes. Recommended: 0.08.")]
    private float sideSwitchDuration = 0.08f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Debug
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField]
    [Tooltip("Log hit info for this enemy type.")]
    private bool logEnemyHits = false;

    [SerializeField]
    [Tooltip("Log lane-blocking events. Keep false in production.")]
    private bool logLaneBlocking = false;

    [SerializeField]
    [Tooltip("Log side-switch events for SwitchEnemy debugging.")]
    private bool logPatternActions = false;

    [Header("Visuals (Phase 19)")]
    [SerializeField]
    [Tooltip("Optional child object containing SpriteRenderer and Animator. Replaces root SpriteRenderer.")]
    private VisualRoot visualRoot;

    [Header("Animation (Phase 24+)")]
    [SerializeField]
    [Tooltip("Optional EnemyAnimationController on the Visual child. Auto-detected if empty.")]
    private EnemyAnimationController enemyAnimationController;

    // ──────────────────────────────────────────────────────────────────────────
    // Runtime data
    // ──────────────────────────────────────────────────────────────────────────

    private Transform playerTransform;
    private int       currentHealth;

    /// <summary>True while knockback movement is running (Normal enemies).</summary>
    private bool isKnockedBack = false;

    /// <summary>
    /// True for the entire duration of a side-switch crossing.
    /// Prevents Game Over collision and extra TakeHit calls.
    /// </summary>
    private bool isSwitchingSide = false;

    /// <summary>
    /// Game Over is suppressed until this time, giving a brief post-landing grace period.
    /// </summary>
    private float noGameOverUntil = 0f;

    /// <summary>Lane blocking is skipped while Time.time is below this value.</summary>
    private float ignoreLaneBlockingUntil = 0f;

    private bool wasBlockedLastFrame = false;

    /// <summary>Set to true the instant the enemy is defeated. Stops movement and collision threat.</summary>
    private bool isDefeated = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Public properties
    // ──────────────────────────────────────────────────────────────────────────

    public SpawnSide         Side          { get; private set; }
    public EnemyBehaviorType BehaviorType  => behaviorType;
    public int               ScoreValue    => scoreValue;
    public int               CurrentHealth => currentHealth;
    public int               MaxHealth     => maxHealth;
    /// <summary>True once MakeHarmless() has been called (health hit 0). Enemy cannot kill or block lanes.</summary>
    public bool              IsDefeated    => isDefeated;
    /// <summary>True only while this enemy is a valid target for a player attack.</summary>
    public bool              CanReceiveHit => !isDefeated && !isSwitchingSide;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        currentHealth = maxHealth;

        if (visualRoot == null)
        {
            visualRoot = GetComponentInChildren<VisualRoot>();
        }

        if (enemyAnimationController == null)
        {
            enemyAnimationController = GetComponentInChildren<EnemyAnimationController>();
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;
        if (playerTransform == null) return;

        // Pause movement during knockback or active side-switch crossing.
        if (isKnockedBack || isSwitchingSide) return;

        // Lane blocking – suppressed during crossing window.
        bool laneBlockingActive = enableLaneBlocking && Time.time >= ignoreLaneBlockingUntil;
        if (laneBlockingActive && IsBlockedByFrontEnemy())
        {
            if (logLaneBlocking && !wasBlockedLastFrame)
            {
                Debug.Log($"[{gameObject.name}] Lane blocked by front enemy.");
            }
            wasBlockedLastFrame = true;
            return;
        }
        wasBlockedLastFrame = false;

        // Move toward the player's current position every frame.
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Suppress Game Over when enemy is already defeated (death animation playing).
        if (isDefeated) return;
        // Suppress Game Over during crossing and during the post-landing grace period.
        if (isSwitchingSide) return;
        if (Time.time < noGameOverUntil) return;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.IsDemoMode)
            {
                HandleDemoPlayerContact();
                return;
            }

            GameManager.Instance.GameOver();
        }
    }

    private void HandleDemoPlayerContact()
    {
        MakeHarmless();
        float delay = PlayDeathAnimationIfAvailable();
        Destroy(gameObject, Mathf.Max(delay, 0.1f));
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Lane blocking helpers  (Phase 9, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    private bool IsBlockedByFrontEnemy()
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        float myX             = transform.position.x;
        float closestGap      = float.MaxValue;
        bool  foundFrontEnemy = false;

        foreach (Enemy other in allEnemies)
        {
            if (other == this) continue;
            if (other.Side != Side) continue;
            if (other.IsDefeated) continue;

            float otherX = other.transform.position.x;
            if (!IsSameSideEnemyInFront(otherX, myX)) continue;

            float gap = DistanceTo(other);
            if (gap < closestGap)
            {
                closestGap      = gap;
                foundFrontEnemy = true;
            }
        }

        return foundFrontEnemy && closestGap < minSpacingFromFrontEnemy;
    }

    private bool IsSameSideEnemyInFront(float otherX, float myX)
    {
        return Side == SpawnSide.Left ? otherX > myX : otherX < myX;
    }

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
    /// Wires up the player reference and records the spawn side.
    /// </summary>
    public void Initialize(Transform player, SpawnSide side)
    {
        playerTransform = player;
        Side            = side;
        UpdateVisualFacingTowardPlayer();
    }

    /// <summary>
    /// Overrides the enemy's move speed at spawn time.
    /// All enemy types should receive the SAME global speed.
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API – Phase 7 (preserved + Phase 12 branch)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by PlayerCombat when a valid hit lands.
    /// Ignores the hit safely while the enemy is switching sides.
    /// Returns true if defeated, false if still alive.
    /// </summary>
    public bool TakeHit()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return false;
        if (IsDefeated) return false;

        // Block extra hits during active side-switch to prevent double triggers.
        if (isSwitchingSide) return false;

        currentHealth--;

        if (logEnemyHits)
        {
            Debug.Log($"[{gameObject.name}] Hit! HP: {currentHealth}/{maxHealth}");
        }

        // Optional visual feedback for pattern enemies
        if ((behaviorType == EnemyBehaviorType.AlternatingThreeHit || behaviorType == EnemyBehaviorType.SwitchSideOnHit) && currentHealth > 0)
        {
            SpriteRenderer sr = null;
            if (visualRoot != null && visualRoot.SpriteRenderer != null)
            {
                sr = visualRoot.SpriteRenderer;
            }
            else
            {
                sr = GetComponent<SpriteRenderer>();
            }

            if (sr != null)
            {
                // Darken slightly on each hit to show progression
                sr.color = new Color(sr.color.r * 0.75f, sr.color.g * 0.75f, sr.color.b * 0.75f, sr.color.a);
            }
        }

        if (currentHealth <= 0)
        {
            // Stop movement and disable collision threat immediately.
            MakeHarmless();
            return true; // Defeated — caller handles effect, animation, and destroy.
        }

        // Still alive — choose reaction.
        // Play non-lethal hit animation before handling knockback/switch.
        UpdateVisualFacingTowardPlayer();
        enemyAnimationController?.PlayBack();

        if (behaviorType == EnemyBehaviorType.SwitchSideOnHit || behaviorType == EnemyBehaviorType.AlternatingThreeHit)
        {
            TriggerSideSwitch();
        }
        else
        {
            TriggerKnockback();
        }
        return false;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Defeat helpers (Phase 24+)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called immediately when health reaches 0.
    /// Stops movement, disables the collider, and marks this enemy as harmless
    /// so it cannot trigger Game Over or block lanes while the death animation plays.
    /// </summary>
    private void MakeHarmless()
    {
        isDefeated = true;

        // Disable collider so the enemy cannot trigger Game Over during death anim.
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Visual facing helpers (Phase 24+)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Gets the direction (1 for right, -1 for left) the enemy should face to look at the player.
    /// </summary>
    private int GetFacingDirectionTowardPlayer()
    {
        if (playerTransform != null)
        {
            if (playerTransform.position.x > transform.position.x) return 1;
            if (playerTransform.position.x < transform.position.x) return -1;
        }
        
        return Side == SpawnSide.Left ? 1 : -1; // Default: face center
    }

    /// <summary>
    /// Updates the visual's facing direction to look at the player.
    /// </summary>
    private void UpdateVisualFacingTowardPlayer()
    {
        if (enemyAnimationController != null)
        {
            enemyAnimationController.SetFacingDirection(GetFacingDirectionTowardPlayer());
        }
    }

    /// <summary>
    /// Determines the visual death drift direction based on the player's position.
    /// </summary>
    private int GetDeathDriftDirection()
    {
        if (playerTransform != null)
        {
            if (transform.position.x < playerTransform.position.x)
                return -1; // Left of player, drift further left
            if (transform.position.x > playerTransform.position.x)
                return 1;  // Right of player, drift further right
        }
        
        // Fallback to spawn side
        return Side == SpawnSide.Left ? -1 : 1;
    }

    /// <summary>
    /// Triggers the death animation if an EnemyAnimationController is present.
    /// Returns the time (in seconds) the caller should wait before destroying this object.
    /// Returns 0 if no animation controller is available (destroy immediately).
    /// </summary>
    public float PlayDeathAnimationIfAvailable()
    {
        if (enemyAnimationController != null)
        {
            UpdateVisualFacingTowardPlayer();
            int dir = GetDeathDriftDirection();
            return enemyAnimationController.PlayDieAndGetDuration(dir);
        }
        return 0f;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Knockback  (Phase 7, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    private void TriggerKnockback()
    {
        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;

        float directionX        = (Side == SpawnSide.Left) ? -1f : 1f;
        Vector3 knockbackOffset = new Vector3(directionX * knockbackDistance, 0f, 0f);

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
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Side-switch placement helpers  (Phase 12 Hotfix v2)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the X half-width of this enemy using Collider2D, then SpriteRenderer,
    /// then a safe fallback of 0.35 units.
    /// </summary>
    private float GetHalfWidth()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) return col.bounds.extents.x;

        if (visualRoot != null && visualRoot.SpriteRenderer != null)
            return visualRoot.SpriteRenderer.bounds.extents.x;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.extents.x;

        return 0.35f;
    }

    /// <summary>
    /// Returns the X half-width of the player using Collider2D, then SpriteRenderer,
    /// then a safe fallback of 0.35 units.
    /// </summary>
    private float GetPlayerHalfWidth()
    {
        if (playerTransform == null) return 0.35f;

        Collider2D col = playerTransform.GetComponent<Collider2D>();
        if (col != null) return col.bounds.extents.x;

        SpriteRenderer sr = playerTransform.GetComponent<SpriteRenderer>();
        if (sr != null) return sr.bounds.extents.x;

        return 0.35f;
    }

    /// <summary>
    /// Absolute centre-to-centre distance from this enemy to the player at this moment.
    /// </summary>
    private float GetCurrentCenterDistanceToPlayer()
    {
        if (playerTransform == null) return 1.5f;
        return Mathf.Abs(transform.position.x - playerTransform.position.x);
    }

    /// <summary>
    /// Minimum safe centre-to-centre distance after landing:
    ///   sideSwitchGap + playerHalfWidth + enemyHalfWidth
    /// This guarantees no collider overlap with the player.
    /// </summary>
    private float CalculateSafeMinSwitchDistance()
    {
        return sideSwitchGap + GetPlayerHalfWidth() + GetHalfWidth();
    }

    /// <summary>
    /// Maximum attackable centre-to-centre distance after landing:
    ///   targetFollowupMaxDistance - rehitMargin
    /// The player's attack range is approximated by targetFollowupMaxDistance.
    /// rehitMargin keeps the landing slightly inside that range for reliability.
    /// </summary>
    private float CalculateMaxFollowupDistance()
    {
        float safeMin = CalculateSafeMinSwitchDistance();
        float rawMax  = targetFollowupMaxDistance - rehitMargin;
        // rawMax must always be at least safeMin to avoid an impossible clamp range.
        return Mathf.Max(rawMax, safeMin);
    }

    /// <summary>
    /// Finds the closest alive, non-self, non-switching enemy that is on the given side
    /// and whose X position is "in front" (closer to the player) on that side.
    /// Returns null if no such enemy exists.
    /// </summary>
    private Enemy FindClosestFrontEnemyOnSide(SpawnSide targetSide)
    {
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        float  playerX   = (playerTransform != null) ? playerTransform.position.x : 0f;
        Enemy  closest   = null;
        float  closestDist = float.MaxValue;

        foreach (Enemy other in allEnemies)
        {
            if (other == this) continue;
            if (other.IsDefeated) continue;
            if (other.isSwitchingSide) continue; // ignore mid-crossing enemies

            // Must occupy the target side as determined by actual X position.
            float otherX = other.transform.position.x;
            bool onTargetSide = (targetSide == SpawnSide.Right)
                                    ? otherX > playerX
                                    : otherX < playerX;
            if (!onTargetSide) continue;

            float dist = Mathf.Abs(otherX - playerX);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest     = other;
            }
        }

        return closest;
    }

    /// <summary>
    /// Computes the best X coordinate for the SwitchEnemy to land on after switching
    /// to newSide, taking into account:
    ///   1. Mirroring: distance measured from PLAYER centre, not from enemy current X.
    ///   2. Clamping: between safeMinDistance and maxFollowupDistance.
    ///   3. Queue awareness: keeps spacing from the front enemy already on the target side.
    /// </summary>
    private float CalculateQueueAwareSwitchTargetX(SpawnSide newSide)
    {
        float playerX    = (playerTransform != null) ? playerTransform.position.x : 0f;
        float safeMin    = CalculateSafeMinSwitchDistance();
        float maxFollowup = CalculateMaxFollowupDistance();

        // Mirror the current approach distance, clamped to the attackable window.
        float currentDist  = GetCurrentCenterDistanceToPlayer();
        float desiredDist  = Mathf.Clamp(currentDist, safeMin, maxFollowup);

        // Candidate landing X (mirrored from player).
        float sign       = (newSide == SpawnSide.Right) ? 1f : -1f;
        float candidateX = playerX + sign * desiredDist;

        // ── Queue-awareness: check for existing enemies on the target side ──────
        Enemy frontEnemy = FindClosestFrontEnemyOnSide(newSide);
        if (frontEnemy != null)
        {
            float frontX = frontEnemy.transform.position.x;

            if (newSide == SpawnSide.Right)
            {
                // Valid landing zone on the right: [playerX+safeMin  …  frontX-minSpacingOnTargetSide]
                float lowerBound = playerX + safeMin;
                float upperBound = frontX  - minSpacingOnTargetSide;

                if (upperBound >= lowerBound)
                {
                    // Room exists — clamp into the valid zone.
                    candidateX = Mathf.Clamp(candidateX, lowerBound, upperBound);
                }
                else
                {
                    // Not enough room: land at the safe minimum — lane blocking will
                    // naturally push the existing front enemy outward each frame.
                    candidateX = lowerBound;
                }
            }
            else // Left
            {
                // Valid landing zone on the left: [frontX+minSpacingOnTargetSide  …  playerX-safeMin]
                float lowerBound = frontX  + minSpacingOnTargetSide;
                float upperBound = playerX - safeMin;

                if (lowerBound <= upperBound)
                {
                    candidateX = Mathf.Clamp(candidateX, lowerBound, upperBound);
                }
                else
                {
                    candidateX = upperBound;
                }
            }
        }

        return candidateX;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Side-switch coroutine  (Phase 12 Hotfix v2)
    // ──────────────────────────────────────────────────────────────────────────

    private void TriggerSideSwitch()
    {
        StartCoroutine(SideSwitchRoutine());
    }

    /// <summary>
    /// Moves the SwitchEnemy to the opposite side of the player using a
    /// mirrored, clamped, queue-aware target X.
    ///
    /// Key properties:
    ///   - isSwitchingSide blocks Game Over and extra hits during crossing.
    ///   - noGameOverUntil adds a post-landing grace period.
    ///   - Side is flipped BEFORE movement so hit detection is correct from frame 1.
    ///   - Landing X is computed from player centre (not enemy position).
    ///   - Distance is clamped between safeMinDistance and maxFollowupDistance.
    ///   - Existing enemies on the target side are respected via FindClosestFrontEnemyOnSide.
    /// </summary>
    private IEnumerator SideSwitchRoutine()
    {
        isSwitchingSide = true;

        // Suppress lane blocking for the crossing window + small buffer.
        ignoreLaneBlockingUntil = Time.time + sideSwitchDuration + 0.05f;

        SpawnSide oldSide = Side;
        SpawnSide newSide = (Side == SpawnSide.Left) ? SpawnSide.Right : SpawnSide.Left;

        // Flip Side BEFORE movement so targeting and lane blocking are immediately correct.
        Side = newSide;

        if (logPatternActions)
        {
            Debug.Log($"[{gameObject.name}] Switching {oldSide} → {newSide}. " +
                      $"safeMin={CalculateSafeMinSwitchDistance():F2} " +
                      $"maxFollowup={CalculateMaxFollowupDistance():F2}");
        }

        // Compute queue-aware, clamped, mirrored landing X.
        Vector3 startPos  = transform.position;
        float   targetX   = CalculateQueueAwareSwitchTargetX(newSide);
        Vector3 targetPos = new Vector3(targetX, startPos.y, startPos.z);

        if (sideSwitchDuration <= 0f)
        {
            transform.position = targetPos;
        }
        else
        {
            float elapsed = 0f;

            while (elapsed < sideSwitchDuration)
            {
                if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
                {
                    isSwitchingSide = false;
                    yield break;
                }

                elapsed += Time.deltaTime;
                float t       = Mathf.Clamp01(elapsed / sideSwitchDuration);
                float smoothT = t * t * (3f - 2f * t); // smooth-step
                transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
                yield return null;
            }

            transform.position = targetPos;
        }

        // Set post-landing grace period to prevent single-frame overlap Game Over.
        noGameOverUntil = Time.time + postSwitchNoGameOverDuration;

        isSwitchingSide = false;

        if (logPatternActions)
        {
            Debug.Log($"[{gameObject.name}] Landed on {newSide} at X={transform.position.x:F2}. " +
                      $"Grace until {noGameOverUntil:F2}.");
        }

        // Phase 24+: Ensure visual is facing the player and return to run animation.
        UpdateVisualFacingTowardPlayer();
        enemyAnimationController?.PlayRun();

        // Normal movement toward player resumes automatically in Update().
    }
}
