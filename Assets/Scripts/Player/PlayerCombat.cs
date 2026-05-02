using UnityEngine;

/// <summary>
/// PlayerCombat – Handles player attack input and enemy destruction.
///
/// Responsibilities:
///   - Listens for A / Left Arrow  → attack LEFT.
///   - Listens for D / Right Arrow → attack RIGHT.
///   - Ignores input when game is over.
///   - Finds all living enemies on the correct side that are within attackRange.
///   - Destroys the closest one (if any).
///
/// Attack detection logic (simple, beginner-friendly):
///   1. Gather all Enemy components currently in the scene.
///   2. Filter to those on the correct side (left X or right X relative to player).
///   3. Among those, find the closest one within attackRange units.
///   4. Destroy it.
///
/// Inspector recommended values:
///   attackRange : 1.5 – 2.0  (units)
///
/// Scene setup:
///   - Attach this script to the "Player" GameObject.
///   - Make sure the Player has the tag "Player" (used by Enemy.cs collision check).
///   - No drag-and-drop references required; everything is found at runtime.
/// </summary>
public class PlayerCombat : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [SerializeField]
    [Tooltip("Maximum distance (units) at which the player can destroy an enemy. Recommended: 1.5–2.0.")]
    private float attackRange = 2.0f;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Do nothing if game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

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
    /// Finds the closest enemy on the given side within attackRange and destroys it.
    /// Does nothing if no valid target exists.
    /// </summary>
    private void TryAttack(Enemy.SpawnSide targetSide)
    {
        // Gather every enemy currently alive in the scene.
        // FindObjectsByType is the Unity 6-compatible replacement for FindObjectsOfType.
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Enemy   closestEnemy    = null;
        float   closestDistance = float.MaxValue;

        float playerX = transform.position.x;

        foreach (Enemy enemy in allEnemies)
        {
            float enemyX = enemy.transform.position.x;

            // ── Side filter ──────────────────────────────────────────────────
            // Left attack: only hit enemies whose X is less than the player's X.
            // Right attack: only hit enemies whose X is greater than the player's X.
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

        // Destroy the closest valid enemy (if one was found).
        if (closestEnemy != null)
        {
            Destroy(closestEnemy.gameObject);
        }
    }
}
