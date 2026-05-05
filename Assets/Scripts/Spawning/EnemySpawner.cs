using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// EnemySpawner – Periodically creates enemy instances in the scene.
///
/// Phase 1 responsibilities (preserved):
///   - Spawns an enemy prefab at a configurable interval.
///   - Randomly picks left or right each time.
///   - Calls Enemy.Initialize() so the new enemy knows its target and side.
///   - Stops spawning when the game is over.
///
/// Phase 5 additions (preserved):
///   - Optional DifficultyManager reference.
///   - Spawn interval driven by DifficultyManager.CurrentSpawnInterval when assigned.
///   - Each spawned enemy's move speed set to DifficultyManager.CurrentEnemySpeed.
///   - Falls back to fixed spawnInterval / prefab default speed if not assigned.
///
/// Phase 8 additions (preserved):
///   - enemyPrefabs[] array supports multiple enemy types (NormalEnemy, HeavyEnemy).
///   - enemyPrefab (single) is kept as a fallback when enemyPrefabs is empty/unset.
///   - ALL spawned enemies receive the SAME global speed from DifficultyManager.
///
/// Phase 14 additions:
///   - enemySpawnWeights[] array supports weighted random selection.
///   - Falls back to equal random selection if weights are missing, wrong length, or zero.
///   - Replaces logSpawnedEnemyType with logSpawnWeights.
///
/// Scene setup:
///   - Attach to the "EnemySpawner" empty GameObject.
///   - Assign Player Transform and DifficultyManager.
///   - enemyPrefabs[0] = NormalEnemy, enemySpawnWeights[0] = 60
///   - enemyPrefabs[1] = HeavyEnemy,  enemySpawnWeights[1] = 25
///   - enemyPrefabs[2] = SwitchEnemy, enemySpawnWeights[2] = 10
///   - enemyPrefabs[3] = PatternEnemy3Hit, enemySpawnWeights[3] = 5
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("Fallback single enemy prefab. Used when enemyPrefabs array is empty or unassigned.")]
    [SerializeField] private GameObject enemyPrefab;

    [Tooltip("Pool of enemy prefabs to spawn randomly. " +
             "Example: [NormalEnemy, HeavyEnemy, SwitchEnemy, PatternEnemy3Hit]")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("Weights for each prefab in enemyPrefabs. Must match the length of enemyPrefabs. " +
             "Example: [60, 25, 10, 5]")]
    [SerializeField] private float[] enemySpawnWeights;

    [Tooltip("The Player GameObject. Drag from the Hierarchy.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Optional: drag DifficultyManager here to enable dynamic spawn interval and enemy speed. " +
             "All enemy types will receive the SAME global speed.")]
    [SerializeField] private DifficultyManager difficultyManager;

    [Header("Spawn Settings (fallback when DifficultyManager is not assigned)")]
    [Tooltip("Fixed seconds between each enemy spawn. Used only when DifficultyManager is not set.")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("World position where left-side enemies appear.")]
    [SerializeField] private Vector3 leftSpawnPosition  = new Vector3(-7f, 0f, 0f);

    [Tooltip("World position where right-side enemies appear.")]
    [SerializeField] private Vector3 rightSpawnPosition = new Vector3( 7f, 0f, 0f);

    [Header("Debug")]
    [Tooltip("Log which prefab was selected and its spawn weight mode. Keep false in production.")]
    [SerializeField] private bool logSpawnWeights = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private float spawnTimer = 0f;
    private bool hasLoggedWeightWarning = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Update()
    {
        // Stop spawning when game is over.
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        spawnTimer += Time.deltaTime;

        // Use DifficultyManager's dynamic interval if available, otherwise fixed value.
        float currentInterval = (difficultyManager != null)
            ? difficultyManager.CurrentSpawnInterval
            : spawnInterval;

        // Phase 17: Clamp to a safe minimum to prevent infinite loops or impossible pacing.
        currentInterval = Mathf.Max(0.1f, currentInterval);

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
        // ── Resolve which prefab to use ──────────────────────────────────────
        GameObject prefabToSpawn = PickPrefab();
        if (prefabToSpawn == null) return;

        if (playerTransform == null)
        {
            Debug.LogWarning("EnemySpawner: playerTransform is not assigned in the Inspector!");
            return;
        }

        // ── Choose left or right spawn ───────────────────────────────────────
        bool spawnLeft = (Random.value < 0.5f);

        Vector3         spawnPos = spawnLeft ? leftSpawnPosition  : rightSpawnPosition;
        Enemy.SpawnSide side     = spawnLeft ? Enemy.SpawnSide.Left : Enemy.SpawnSide.Right;

        // ── Instantiate ──────────────────────────────────────────────────────
        GameObject newEnemyObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // ── Wire up the enemy component ──────────────────────────────────────
        Enemy enemyScript = newEnemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(playerTransform, side);

            // Apply the SAME global speed to every enemy type.
            if (difficultyManager != null)
            {
                enemyScript.SetMoveSpeed(difficultyManager.CurrentEnemySpeed);
            }
        }
        else
        {
            Debug.LogWarning($"EnemySpawner: Prefab \"{prefabToSpawn.name}\" is missing the Enemy.cs component!");
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Selection logic
    // ──────────────────────────────────────────────────────────────────────────

    private GameObject PickPrefab()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            if (enemyPrefab != null) return enemyPrefab;
            Debug.LogWarning("EnemySpawner: No enemy prefab is assigned. " +
                             "Assign prefabs to enemyPrefabs[] or the single enemyPrefab slot.");
            return null;
        }

        bool useWeights = true;

        if (enemySpawnWeights == null || enemySpawnWeights.Length == 0)
        {
            useWeights = false;
        }
        else if (enemySpawnWeights.Length != enemyPrefabs.Length)
        {
            useWeights = false;
            if (!hasLoggedWeightWarning)
            {
                Debug.LogWarning("EnemySpawner: Spawn weights invalid (length mismatch). Falling back to equal random selection.");
                hasLoggedWeightWarning = true;
            }
        }
        else
        {
            float total = 0f;
            for (int i = 0; i < enemySpawnWeights.Length; i++)
            {
                if (enemyPrefabs[i] != null && enemySpawnWeights[i] > 0f)
                {
                    total += enemySpawnWeights[i];
                }
            }

            if (total <= 0f)
            {
                useWeights = false;
                if (!hasLoggedWeightWarning)
                {
                    Debug.LogWarning("EnemySpawner: Spawn weights invalid (total <= 0). Falling back to equal random selection.");
                    hasLoggedWeightWarning = true;
                }
            }
        }

        return useWeights ? PickWeightedPrefab() : PickEqualRandomPrefab();
    }

    private GameObject PickWeightedPrefab()
    {
        float totalWeight = 0f;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] != null && enemySpawnWeights[i] > 0f)
            {
                totalWeight += enemySpawnWeights[i];
            }
        }

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;
        GameObject lastValid = null;

        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] != null && enemySpawnWeights[i] > 0f)
            {
                cumulative += enemySpawnWeights[i];
                lastValid = enemyPrefabs[i];
                if (roll <= cumulative)
                {
                    if (logSpawnWeights) Debug.Log($"EnemySpawner: Selected {enemyPrefabs[i].name} using weighted spawn.");
                    return enemyPrefabs[i];
                }
            }
        }

        if (logSpawnWeights && lastValid != null) Debug.Log($"EnemySpawner: Selected {lastValid.name} using weighted spawn (fallback).");
        return lastValid;
    }

    private GameObject PickEqualRandomPrefab()
    {
        int validCount = 0;
        for (int i = 0; i < enemyPrefabs.Length; i++)
        {
            if (enemyPrefabs[i] != null) validCount++;
        }

        if (validCount > 0)
        {
            int pick = Random.Range(0, validCount);
            int seen = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] == null) continue;
                if (seen == pick)
                {
                    if (logSpawnWeights) Debug.Log($"EnemySpawner: Selected {enemyPrefabs[i].name} using equal random fallback.");
                    return enemyPrefabs[i];
                }
                seen++;
            }
        }

        if (enemyPrefab != null) return enemyPrefab;
        return null;
    }
}
