using UnityEngine;

/// <summary>
/// GameManager – Core game state controller.
///
/// Phase 1 responsibilities (preserved):
///   - Tracks game over state via IsGameOver.
///   - Provides GameOver() that runs only once.
///
/// Phase 2 additions:
///   - Tracks score and combo.
///   - RegisterHit(baseScore) : increases combo, calculates score gain, logs result.
///   - RegisterMiss()         : resets combo, logs miss.
///   - ResetCombo()           : utility to zero out combo externally if needed.
///   - GameOver() now also logs Final Score and Final Combo.
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
    /// Static reference so any script can reach GameManager without
    /// a serialized field in the Inspector.
    /// </summary>
    public static GameManager Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────────────────
    // State  (private backing fields + public read-only properties)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>True once GameOver() has been called.</summary>
    public bool IsGameOver { get; private set; } = false;

    /// <summary>Current accumulated score.</summary>
    public int Score { get; private set; } = 0;

    /// <summary>Current consecutive-hit streak.</summary>
    public int Combo { get; private set; } = 0;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Singleton setup: destroy duplicates.
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
    /// Called by PlayerCombat when the player destroys an enemy.
    /// Increases combo, calculates score gain with combo multiplier, logs result.
    /// Formula: scoreGain = baseScore + (combo * 10)
    /// </summary>
    public void RegisterHit(int baseScore)
    {
        if (IsGameOver) return;

        // Increase combo first so the first hit already gives Combo: 1.
        Combo++;

        int scoreGain = baseScore + (Combo * 10);
        Score += scoreGain;

        Debug.Log($"Hit! Score: {Score}, Combo: {Combo}, Gain: {scoreGain}");
    }

    /// <summary>
    /// Called by PlayerCombat when the player attacks but hits nothing.
    /// Resets combo and logs the miss.
    /// </summary>
    public void RegisterMiss()
    {
        if (IsGameOver) return;

        Combo = 0;
        Debug.Log("Miss! Combo reset.");
    }

    /// <summary>
    /// Utility to zero out the combo without logging.
    /// Can be called externally if other systems need to reset the streak.
    /// </summary>
    public void ResetCombo()
    {
        Combo = 0;
    }

    /// <summary>
    /// Triggers game over. Safe to call multiple times – only the first call runs.
    /// Logs "Game Over", "Final Score", and "Final Combo".
    /// </summary>
    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        Debug.Log("Game Over");
        Debug.Log($"Final Score: {Score}");
        Debug.Log($"Final Combo: {Combo}");
    }
}
