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
/// Phase 8 additions:
///   - enemyPrefabs[] array supports multiple enemy types (NormalEnemy, HeavyEnemy).
///   - If enemyPrefabs has at least one valid entry, one is chosen randomly each spawn.
///   - enemyPrefab (single) is kept as a fallback when enemyPrefabs is empty/unset.
///   - ALL spawned enemies receive the SAME global speed from DifficultyManager.
///     HeavyEnemy should not move slower or faster — it is harder because it needs 2 hits.
///   - Optional logSpawnedEnemyType for debugging without log spam.
///
/// Scene setup:
///   - Attach to the "EnemySpawner" empty GameObject.
///   - Assign Player Transform.
///   - Assign DifficultyManager (recommended).
///   - Drag NormalEnemy + HeavyEnemy prefabs into the enemyPrefabs array.
///   - Leave the single enemyPrefab slot as optional fallback.
///
/// Inspector recommended values (fallback):
///   spawnInterval      : 1.5  (seconds between spawns)
///   leftSpawnPosition  : (-7, 0, 0)
///   rightSpawnPosition : ( 7, 0, 0)
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
             "Add NormalEnemy at index 0 and HeavyEnemy at index 1. " +
             "Simple equal-chance random selection is used. " +
             "If this array is empty, the single enemyPrefab fallback is used instead.")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Tooltip("The Player GameObject. Drag from the Hierarchy.")]
    [SerializeField] private Transform playerTransform;

    [Tooltip("Optional: drag DifficultyManager here to enable dynamic spawn interval and enemy speed. " +
             "All enemy types will receive the SAME global speed. " +
             "If left empty, fixed spawnInterval and prefab default speed are used.")]
    [SerializeField] private DifficultyManager difficultyManager;

    [Header("Spawn Settings (fallback when DifficultyManager is not assigned)")]
    [Tooltip("Fixed seconds between each enemy spawn. Used only when DifficultyManager is not set.")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Tooltip("World position where left-side enemies appear.")]
    [SerializeField] private Vector3 leftSpawnPosition  = new Vector3(-7f, 0f, 0f);

    [Tooltip("World position where right-side enemies appear.")]
    [SerializeField] private Vector3 rightSpawnPosition = new Vector3( 7f, 0f, 0f);

    [Header("Debug")]
    [Tooltip("Log which prefab was selected each spawn. Keep false in production.")]
    [SerializeField] private bool logSpawnedEnemyType = false;

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

        // Use DifficultyManager's dynamic interval if available, otherwise fixed value.
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
        // ── Resolve which prefab to use ──────────────────────────────────────
        GameObject prefabToSpawn = PickPrefab();
        if (prefabToSpawn == null) return;  // warning already logged inside PickPrefab

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

        if (logSpawnedEnemyType)
        {
            Debug.Log($"EnemySpawner: Spawned enemy prefab: {prefabToSpawn.name} on the {side} side.");
        }

        // ── Wire up the enemy component ──────────────────────────────────────
        Enemy enemyScript = newEnemyObj.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.Initialize(playerTransform, side);

            // Apply the SAME global speed to every enemy type.
            // HeavyEnemy is harder because it needs 2 hits, not because it moves differently.
            if (difficultyManager != null)
            {
                enemyScript.SetMoveSpeed(difficultyManager.CurrentEnemySpeed);
            }
            // If no DifficultyManager, the enemy uses the default moveSpeed from its prefab.
            // Ensure all prefabs have the same default moveSpeed (recommended: 2.5).
        }
        else
        {
            Debug.LogWarning($"EnemySpawner: Prefab \"{prefabToSpawn.name}\" is missing the Enemy.cs component!");
        }
    }

    /// <summary>
    /// Picks a prefab to spawn.
    /// Priority:
    ///   1. enemyPrefabs[] array (if it has at least one valid non-null entry).
    ///   2. Single enemyPrefab fallback.
    ///   3. Null + warning if neither is assigned.
    /// Selection from enemyPrefabs is simple equal-chance random.
    /// </summary>
    private GameObject PickPrefab()
    {
        // Build a list of valid (non-null) entries from the array.
        if (enemyPrefabs != null && enemyPrefabs.Length > 0)
        {
            // Collect valid non-null prefabs.
            int validCount = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++)
            {
                if (enemyPrefabs[i] != null) validCount++;
            }

            if (validCount > 0)
            {
                // Pick a random index, skip nulls.
                int pick = Random.Range(0, validCount);
                int seen = 0;
                for (int i = 0; i < enemyPrefabs.Length; i++)
                {
                    if (enemyPrefabs[i] == null) continue;
                    if (seen == pick) return enemyPrefabs[i];
                    seen++;
                }
            }
        }

        // Fallback: single prefab.
        if (enemyPrefab != null) return enemyPrefab;

        Debug.LogWarning("EnemySpawner: No enemy prefab is assigned. " +
                         "Assign prefabs to enemyPrefabs[] or the single enemyPrefab slot.");
        return null;
    }
}
