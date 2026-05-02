using UnityEngine;

/// <summary>
/// GameManager – Core game state controller.
///
/// Responsibilities:
///   - Tracks whether the game is currently running or over.
///   - Exposes a public IsGameOver property that other scripts read.
///   - Provides a public GameOver() method; other scripts call this
///     when they detect a loss condition (e.g. enemy touches player).
///   - GameOver() is guarded so it executes only once per session.
///
/// Scene setup:
///   Attach this script to the "GameManager" empty GameObject in MainScene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Static reference so any script can call GameManager.Instance.IsGameOver
    /// without needing a serialized field link in the Inspector.
    /// </summary>
    public static GameManager Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// True once GameOver() has been called.
    /// Other scripts (Enemy, EnemySpawner, PlayerCombat) check this each frame.
    /// </summary>
    public bool IsGameOver { get; private set; } = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Set up singleton. If a duplicate exists, destroy it.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this from any script that detects a game-over condition.
    /// Safe to call multiple times – only the first call does anything.
    /// </summary>
    public void GameOver()
    {
        // Guard: run only once.
        if (IsGameOver) return;

        IsGameOver = true;
        Debug.Log("Game Over");
    }
}
