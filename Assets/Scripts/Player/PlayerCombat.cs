using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerCombat – Handles player attack input, hit/miss registration, and stun.
///
/// Phase 1 responsibilities (preserved):
///   - A / Left Arrow  = attack LEFT.
///   - D / Right Arrow = attack RIGHT.
///   - Ignores input when game is over.
///   - Finds the closest in-range enemy on the correct side and destroys it.
///
/// Phase 2 additions:
///   - On a HIT  : calls GameManager.RegisterHit(enemy.ScoreValue) before destroying.
///   - On a MISS : calls GameManager.RegisterMiss() then stuns the player briefly.
///   - While stunned, all input is ignored.
///   - Stun duration is configurable in the Inspector.
///   - Logs "Player stunned." and "Player recovered." to the Console.
///
/// Miss definition:
///   The player presses an attack key but there is no enemy on that side within attackRange.
///
/// Inspector recommended values:
///   attackRange   : 1.5 – 2.0  (units)
///   stunDuration  : 0.4        (seconds)
///
/// Scene setup:
///   - Attach this script to the "Player" GameObject.
///   - Make sure the Player has the tag "Player" (used by Enemy.cs collision check).
///   - No drag-and-drop references needed; everything is found at runtime.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Maximum distance (units) at which the player can destroy an enemy. Recommended: 1.5–2.0.")]
    private float attackRange = 2.0f;

    [SerializeField]
    [Tooltip("Seconds the player cannot attack after a miss. Recommended: 0.4.")]
    private float stunDuration = 0.4f;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// True while the player is serving a stun penalty after a miss.
    /// All attack input is ignored during this window.
    /// </summary>
    private bool isStunned = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Ignore input when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Ignore input while stunned.
        if (isStunned) return;

        // ── Attack LEFT ──────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            TryAttack(Enemy.SpawnSide.Left);
        }

        // ── Attack RIGHT ─────────────────────────────────────────────────────
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            TryAttack(Enemy.SpawnSide.Right);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Attack logic
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tries to destroy the closest valid enemy on the given side.
    /// On hit  → registers the hit with GameManager and destroys the enemy.
    /// On miss → registers the miss with GameManager and starts stun.
    /// </summary>
    private void TryAttack(Enemy.SpawnSide targetSide)
    {
        // Gather every enemy alive in the scene.
        // FindObjectsByType is the Unity 6-compatible API (replaces FindObjectsOfType).
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy closestEnemy    = null;
        float closestDistance = float.MaxValue;

        float playerX = transform.position.x;

        foreach (Enemy enemy in allEnemies)
        {
            float enemyX = enemy.transform.position.x;

            // ── Side filter ──────────────────────────────────────────────────
            // Left attack  : only enemies with X < player X.
            // Right attack : only enemies with X > player X.
            bool isOnCorrectSide =
                (targetSide == Enemy.SpawnSide.Left  && enemyX < playerX) ||
                (targetSide == Enemy.SpawnSide.Right && enemyX > playerX);

            if (!isOnCorrectSide) continue;

            // ── Range filter ─────────────────────────────────────────────────
            float distance = Vector2.Distance(transform.position, enemy.transform.position);
            if (distance > attackRange) continue;

            // ── Closest check ────────────────────────────────────────────────
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy    = enemy;
            }
        }

        // ── Hit ──────────────────────────────────────────────────────────────
        if (closestEnemy != null)
        {
            // Register the hit BEFORE destroying so ScoreValue is still accessible.
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterHit(closestEnemy.ScoreValue);
            }

            Destroy(closestEnemy.gameObject);
        }
        // ── Miss ─────────────────────────────────────────────────────────────
        else
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterMiss();
            }

            // Start a short stun so the player cannot immediately attack again.
            StartCoroutine(StunRoutine());
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Stun coroutine
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets isStunned to true for stunDuration seconds, then clears it.
    /// Logs the stun start and end to the Console.
    /// </summary>
    private IEnumerator StunRoutine()
    {
        isStunned = true;
        Debug.Log("Player stunned.");

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        Debug.Log("Player recovered.");
    }
}
