using UnityEngine;

/// <summary>
/// EnemySpawner – Periodically creates enemy instances in the scene.
///
/// Phase 1 responsibilities (preserved):
///   - Spawns the enemy prefab at a configurable interval.
///   - Randomly picks left or right each time.
///   - Calls Enemy.Initialize() so the new enemy knows its target and side.
///   - Stops spawning when the game is over.
///
/// Phase 5 additions:
///   - Optional DifficultyManager reference.
///   - If assigned, the spawn interval comes from DifficultyManager.CurrentSpawnInterval
///     (checked every spawn cycle, so it tightens dynamically as time passes).
///   - If assigned, each spawned enemy's move speed is set to
///     DifficultyManager.CurrentEnemySpeed via Enemy.SetMoveSpeed().
///   - If DifficultyManager is NOT assigned, behaves exactly as Phase 1 (fallback).
///
/// Scene setup:
///   - Attach this script to the "EnemySpawner" empty GameObject in MainScene.
///   - Drag the Enemy prefab from Assets/Prefabs into the "Enemy Prefab" slot.
///   - Drag the Player GameObject into the "Player Transform" slot.
///   - Drag the DifficultyManager component into the "Difficulty Manager" slot.
///
/// Inspector recommended values (fallback, used when no DifficultyManager):
///   spawnInterval     : 1.5  (seconds between spawns)
///   leftSpawnPosition : (-7, 0, 0)
///   rightSpawnPosition: ( 7, 0, 0)
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The Enemy prefab to instantiate. Drag from Assets/Prefabs.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("The Player GameObject. Drag from the Hierarchy.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Optional: drag DifficultyManager here to enable dynamic spawn interval and enemy speed. " +
             "If left empty, the fixed spawnInterval below is used.")]
    [SerializeField] private DifficultyManager difficultyManager;

    [Header("Spawn Settings (fallback when DifficultyManager is not assigned)")]
    [Tooltip("Fixed seconds between each enemy spawn. Used only when DifficultyManager is not set.")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("World position where left-side enemies appear.")]
    [SerializeField] private Vector3 leftSpawnPosition  = new Vector3(-7f, 0f, 0f);

    [Tooltip("World position where right-side enemies appear.")]
    [SerializeField] private Vector3 rightSpawnPosition = new Vector3( 7f, 0f, 0f);

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private float spawnTimer = 0f;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Stop spawning when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        spawnTimer += Time.deltaTime;

        // Use DifficultyManager's dynamic interval if available, otherwise use fixed value.
        float currentInterval = (difficultyManager != null)
            ? difficultyManager.CurrentSpawnInterval
            : spawnInterval;

        if (spawnTimer >= currentInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Spawning logic
    // ──────────────────────────────────────────────────────────────────────────

    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: enemyPrefab is not assigned in the Inspector!");
            return;
        }

        if (playerTransform == null)
        {
            Debug.LogWarning("EnemySpawner: playerTransform is not assigned in the Inspector!");
            return;
        }

        // Randomly choose left or right.
        bool spawnLeft = (Random.value < 0.5f);

        Vector3 spawnPos        = spawnLeft ? leftSpawnPosition  : rightSpawnPosition;
        Enemy.SpawnSide side    = spawnLeft ? Enemy.SpawnSide.Left : Enemy.SpawnSide.Right;

        // Instantiate the prefab at the chosen position.
        GameObject newEnemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Wire up the enemy.
        Enemy enemyScript = newEnemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(playerTransform, side);

            // Override move speed with current difficulty value if available.
            if (difficultyManager != null)
            {
                enemyScript.SetMoveSpeed(difficultyManager.CurrentEnemySpeed);
            }
            // If no DifficultyManager, the enemy uses the default moveSpeed from its prefab.
        }
        else
        {
            Debug.LogWarning("EnemySpawner: Enemy prefab is missing the Enemy.cs component!");
        }
    }
}
