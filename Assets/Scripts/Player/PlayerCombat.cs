using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerCombat – Handles player attack input, hit/miss registration, stun,
/// and Phase 4 visual feedback.
///
/// Phase 1 responsibilities (preserved):
///   - A / Left Arrow  = attack LEFT.
///   - D / Right Arrow = attack RIGHT.
///   - Ignores input when game is over.
///   - Finds the closest in-range enemy on the correct side and destroys it.
///
/// Phase 2 responsibilities (preserved):
///   - On MISS : calls GameManager.RegisterMiss() then stuns the player briefly.
///   - While stunned, all input is ignored.
///   - Logs "Player stunned." and "Player recovered."
///
/// Phase 4 additions (preserved):
///   - Attack direction indicator: briefly shows a sprite left or right of the player.
///   - Hit effect: optionally instantiates a prefab (with TemporaryEffect.cs) at enemy pos.
///   - Stun color: player sprite tints to stunColor while stunned, restores normalColor after.
///   - Camera shake: light shake on miss, stronger shake on game over.
///
/// Phase 6 update (preserved):
///   - RegisterSuccessfulHit() increases combo on every valid hit.
///   - RegisterEnemyDefeated() awards score only when the enemy dies.
///
/// Phase 7 update (preserved):
///   - Hit branch calls enemy.TakeHit() to apply damage.
///   - If TakeHit() returns true  (defeated): award score, spawn effect, destroy.
///   - If TakeHit() returns false (alive):    combo still counts, no score, no destroy.
///     Enemy handles its own knockback internally.
///
/// Phase 10 update:
///   - Calls PlayerMovement.ShiftLeft() / ShiftRight() on every accepted attack input.
///   - Shift happens regardless of hit or miss (as long as not stunned / game over).
///   - PlayerMovement is auto-detected via GetComponent if not assigned in Inspector.
///
/// Inspector recommended values:
///   attackRange            : 1.5 – 2.0
///   stunDuration           : 0.4
///   attackIndicatorDuration: 0.15
///   stunColor              : red   (255, 80, 80, 255)
///   normalColor            : white (255, 255, 255, 255)
///
/// All feedback fields are optional. If unassigned, gameplay continues without errors.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Combat (Phase 1 / 2, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Combat")]
    [SerializeField]
    [Tooltip("Maximum distance (units) at which the player can destroy an enemy. Recommended: 1.5–2.0.")]
    private float attackRange = 2.0f;

    [SerializeField]
    [Tooltip("Seconds the player cannot attack after a miss. Recommended: 0.4.")]
    private float stunDuration = 0.4f;

    [Header("Player Movement (Phase 10)")]
    [Tooltip("PlayerMovement component for attack-momentum shifts. " +
             "Auto-detected from this GameObject if left empty.")]
    [SerializeField] private PlayerMovement playerMovement;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Attack indicator (Phase 4)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Attack Indicators (optional)")]
    [Tooltip("GameObject shown briefly when the player attacks LEFT. Can be a child sprite.")]
    [SerializeField] private GameObject leftAttackIndicator;

    [Tooltip("GameObject shown briefly when the player attacks RIGHT. Can be a child sprite.")]
    [SerializeField] private GameObject rightAttackIndicator;

    [Tooltip("How long (seconds) the attack indicator stays visible. Recommended: 0.15.")]
    [SerializeField] private float attackIndicatorDuration = 0.15f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Hit effect (Phase 4)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Hit Effect (optional)")]
    [Tooltip("Prefab instantiated at the enemy position on a successful hit. " +
             "Must have TemporaryEffect.cs attached. Leave empty to skip.")]
    [SerializeField] private GameObject hitEffectPrefab;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Stun color (Phase 4)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Stun Visual (optional)")]
    [Tooltip("Player SpriteRenderer for color tinting. Auto-detected if left empty.")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;

    [Tooltip("Player sprite color while stunned. Recommended: red (255, 80, 80, 255).")]
    [SerializeField] private Color stunColor = new Color(1f, 0.31f, 0.31f, 1f);

    [Tooltip("Normal player sprite color (restored after stun). Usually white.")]
    [SerializeField] private Color normalColor = Color.white;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Camera shake (Phase 4)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Camera Shake (optional)")]
    [Tooltip("Drag the Main Camera's CameraShake component here to enable shake.")]
    [SerializeField] private CameraShake cameraShake;

    [Tooltip("Shake duration on a miss (seconds).")]
    [SerializeField] private float missShakeDuration = 0.15f;

    [Tooltip("Shake strength on a miss (units).")]
    [SerializeField] private float missShakeStrength = 0.08f;

    [Tooltip("Shake duration on game over (seconds).")]
    [SerializeField] private float gameOverShakeDuration = 0.25f;

    [Tooltip("Shake strength on game over (units).")]
    [SerializeField] private float gameOverShakeStrength = 0.15f;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>True while the player is serving a stun penalty after a miss.</summary>
    private bool isStunned = false;

    // Coroutine handles so we can cancel a running indicator early if needed.
    private Coroutine leftIndicatorCoroutine;
    private Coroutine rightIndicatorCoroutine;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Auto-detect SpriteRenderer if not assigned in Inspector.
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Auto-detect PlayerMovement if not assigned in Inspector.
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        // Make sure indicators start hidden.
        if (leftAttackIndicator  != null) leftAttackIndicator.SetActive(false);
        if (rightAttackIndicator != null) rightAttackIndicator.SetActive(false);
    }

    private void Start()
    {
        // Subscribe to the game-over event so we can trigger the stronger shake.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent -= HandleGameOver;
        }
    }

    private void Update()
    {
        // Ignore input when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Ignore input while stunned.
        if (isStunned) return;

        // ── Attack LEFT ──────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            // Phase 10: shift before indicator + attack so position is updated.
            playerMovement?.ShiftLeft();
            ShowAttackIndicator(Enemy.SpawnSide.Left);
            TryAttack(Enemy.SpawnSide.Left);
        }

        // ── Attack RIGHT ─────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            // Phase 10: shift before indicator + attack so position is updated.
            playerMovement?.ShiftRight();
            ShowAttackIndicator(Enemy.SpawnSide.Right);
            TryAttack(Enemy.SpawnSide.Right);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Attack logic
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tries to hit the closest valid enemy on the given side.
    ///
    /// On valid hit:
    ///   1. RegisterSuccessfulHit()         – always increments combo.
    ///   2. enemy.TakeHit()                 – reduces enemy health by 1.
    ///   3a. If defeated: RegisterEnemyDefeated(), spawn effect, Destroy.
    ///   3b. If alive:    no score, no destroy; enemy handles its own knockback.
    ///
    /// On miss → registers miss, starts stun, shakes camera.
    /// </summary>
    private void TryAttack(Enemy.SpawnSide targetSide)
    {
        // Gather every enemy alive in the scene.
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy closestEnemy    = null;
        float closestDistance = float.MaxValue;
        float playerX         = transform.position.x;

        foreach (Enemy enemy in allEnemies)
        {
            float enemyX = enemy.transform.position.x;

            bool isOnCorrectSide =
                (targetSide == Enemy.SpawnSide.Left  && enemyX < playerX) ||
                (targetSide == Enemy.SpawnSide.Right && enemyX > playerX);

            if (!isOnCorrectSide) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance > attackRange) continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy    = enemy;
            }
        }

        // ── Hit ──────────────────────────────────────────────────────────────
        if (closestEnemy != null)
        {
            // Step 1 – Combo always increases on every accurate hit.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterSuccessfulHit();
            }

            // Step 2 – Deal damage and find out whether the enemy is now dead.
            bool defeated = closestEnemy.TakeHit();

            if (defeated)
            {
                // Step 3a – Enemy defeated: award score, spawn effect, destroy.
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterEnemyDefeated(closestEnemy.ScoreValue);
                }

                SpawnHitEffect(closestEnemy.transform.position);
                Destroy(closestEnemy.gameObject);
            }
            else
            {
                // Step 3b – Enemy still alive: show a hit effect but keep the enemy.
                // Combo has already been registered above.
                // The enemy handles its own knockback inside TakeHit().
                SpawnHitEffect(closestEnemy.transform.position);
                // Do NOT award score. Do NOT destroy.
            }
        }
        // ── Miss ─────────────────────────────────────────────────────────────
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterMiss();
            }

            // Camera shake on miss (optional).
            if (cameraShake != null)
            {
                cameraShake.Shake(missShakeDuration, missShakeStrength);
            }

            StartCoroutine(StunRoutine());
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Attack indicator
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Briefly shows the left or right attack indicator sprite then hides it.
    /// </summary>
    private void ShowAttackIndicator(Enemy.SpawnSide side)
    {
        if (side == Enemy.SpawnSide.Left && leftAttackIndicator != null)
        {
            if (leftIndicatorCoroutine != null) StopCoroutine(leftIndicatorCoroutine);
            leftIndicatorCoroutine = StartCoroutine(IndicatorRoutine(leftAttackIndicator));
        }
        else if (side == Enemy.SpawnSide.Right && rightAttackIndicator != null)
        {
            if (rightIndicatorCoroutine != null) StopCoroutine(rightIndicatorCoroutine);
            rightIndicatorCoroutine = StartCoroutine(IndicatorRoutine(rightAttackIndicator));
        }
    }

    private IEnumerator IndicatorRoutine(GameObject indicator)
    {
        indicator.SetActive(true);
        yield return new WaitForSeconds(attackIndicatorDuration);
        indicator.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Hit effect
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Instantiates the hit effect prefab at the given world position if one is assigned.
    /// </summary>
    private void SpawnHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, position, Quaternion.identity);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Stun coroutine  (Phase 2 extended with Phase 4 color tint)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets isStunned, tints the player sprite to stunColor, waits stunDuration,
    /// then restores normal color and clears the stun flag.
    /// </summary>
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        Debug.Log("Player stunned.");

        // Apply stun color tint.
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = stunColor;
        }

        yield return new WaitForSeconds(stunDuration);

        // Restore normal color.
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = normalColor;
        }

        isStunned = false;
        Debug.Log("Player recovered.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Game-over handler
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called once when the game ends. Triggers a stronger camera shake (optional).
    /// </summary>
    private void HandleGameOver(int finalScore, int finalCombo)
    {
        if (cameraShake != null)
        {
            cameraShake.Shake(gameOverShakeDuration, gameOverShakeStrength);
        }
    }
}
