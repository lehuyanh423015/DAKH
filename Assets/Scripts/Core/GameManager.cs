using UnityEngine;

/// <summary>
/// GameManager – Core game state controller.
///
/// Phase 1 responsibilities (preserved):
///   - Tracks game over state via IsGameOver.
///   - Provides GameOver() that runs only once.
///
/// Phase 2 responsibilities (preserved):
///   - Tracks score and combo.
///   - RegisterHit(baseScore) : increases combo, calculates score gain, logs result.
///   - RegisterMiss()         : resets combo, logs miss.
///   - ResetCombo()           : utility to zero out combo externally if needed.
///   - GameOver() logs Final Score and Final Combo.
///
/// Phase 3 additions:
///   - OnScoreComboChanged event: fired whenever score or combo updates.
///   - OnGameOverEvent event: fired when the game ends (carries final score/combo).
///   - UI scripts subscribe to these events to refresh the display.
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
    // Events  (Phase 3)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired after every hit or miss so UI can refresh score/combo display.
    /// Parameters: (currentScore, currentCombo)
    /// </summary>
    public event System.Action<int, int> OnScoreComboChanged;

    /// <summary>
    /// Fired once when the game ends.
    /// Parameters: (finalScore, finalCombo)
    /// </summary>
    public event System.Action<int, int> OnGameOverEvent;

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
    /// Increases combo, calculates score gain with combo multiplier, logs result,
    /// then fires OnScoreComboChanged so the UI refreshes.
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

        // Notify UI.
        OnScoreComboChanged?.Invoke(Score, Combo);
    }

    /// <summary>
    /// Called by PlayerCombat when the player attacks but hits nothing.
    /// Resets combo, logs the miss, then fires OnScoreComboChanged so the UI refreshes.
    /// </summary>
    public void RegisterMiss()
    {
        if (IsGameOver) return;

        Combo = 0;
        Debug.Log("Miss! Combo reset.");

        // Notify UI (combo is now 0, score unchanged).
        OnScoreComboChanged?.Invoke(Score, Combo);
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
    /// Logs "Game Over", "Final Score", and "Final Combo",
    /// then fires OnGameOverEvent so the UI shows the Game Over panel.
    /// </summary>
    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        Debug.Log("Game Over");
        Debug.Log($"Final Score: {Score}");
        Debug.Log($"Final Combo: {Combo}");

        // Notify UI.
        OnGameOverEvent?.Invoke(Score, Combo);
    }
}
