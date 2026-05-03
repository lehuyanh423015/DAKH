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
///   - RegisterMiss()   : resets combo immediately, logs miss.
///   - ResetCombo()     : utility zero-out without logging.
///   - GameOver()       : logs Final Score / Combo, fires OnGameOverEvent.
///
/// Phase 3 additions (preserved):
///   - OnScoreComboChanged event – fired whenever score or combo changes.
///   - OnGameOverEvent event     – fired once when the game ends.
///
/// Phase 6 refactor – Combo-by-hit with timeout:
///   OLD: combo increased only when an enemy was defeated (RegisterHit).
///   NEW: combo increases on every accurate hit (RegisterSuccessfulHit),
///        score is awarded separately when an enemy is defeated (RegisterEnemyDefeated).
///        Combo expires automatically after comboWindowDuration without a new hit.
///   BACKWARD COMPAT: RegisterHit(baseScore) wrapper calls both methods so
///        existing single-hit enemies continue working without code changes.
///
/// Scene setup:
///   Attach this script to the "GameManager" empty GameObject in MainScene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────────────────────────────────

    public static GameManager Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────────────────
    // Events  (Phase 3, preserved)
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Fired after every combo/score change. Parameters: (score, combo).</summary>
    public event System.Action<int, int> OnScoreComboChanged;

    /// <summary>Fired once when the game ends. Parameters: (finalScore, finalCombo).</summary>
    public event System.Action<int, int> OnGameOverEvent;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields  (Phase 6)
    // ──────────────────────────────────────────────────────────────────────────

    [Tooltip("Seconds after the last successful hit before the combo expires. Recommended: 1.0.")]
    [SerializeField] private float comboWindowDuration = 1.0f;

    // ──────────────────────────────────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>True once GameOver() has been called.</summary>
    public bool IsGameOver { get; private set; } = false;

    /// <summary>Current accumulated score.</summary>
    public int Score { get; private set; } = 0;

    /// <summary>Current consecutive-hit streak.</summary>
    public int Combo { get; private set; } = 0;

    /// <summary>Exposes the combo window so UI or other systems can read it.</summary>
    public float ComboWindowDuration => comboWindowDuration;

    // ── Phase 6 combo-timeout tracking ──
    private float lastSuccessfulHitTime = float.NegativeInfinity;
    private bool  hasActiveCombo        = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        // Only check timeout while a combo is active and game is still running.
        if (IsGameOver)          return;
        if (!hasActiveCombo)     return;

        if (Time.time - lastSuccessfulHitTime > comboWindowDuration)
        {
            // Combo window elapsed – reset silently and notify UI once.
            Combo         = 0;
            hasActiveCombo = false;
            Debug.Log("Combo expired.");
            OnScoreComboChanged?.Invoke(Score, Combo);
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Call when the player lands an accurate hit on any enemy (even a multi-hit one).
    /// Increases combo and starts / refreshes the combo window timer.
    /// Does NOT add score – call RegisterEnemyDefeated() separately when the enemy dies.
    /// </summary>
    public void RegisterSuccessfulHit()
    {
        if (IsGameOver) return;

        if (!hasActiveCombo)
        {
            // First hit of a new streak.
            Combo = 1;
        }
        else if (Time.time - lastSuccessfulHitTime <= comboWindowDuration)
        {
            // Still inside the window – extend the streak.
            Combo++;
        }
        else
        {
            // Window already expired between the Update() check and this call
            // (edge case on the same frame). Treat as a fresh streak.
            Combo = 1;
        }

        hasActiveCombo        = true;
        lastSuccessfulHitTime = Time.time;

        Debug.Log($"Hit registered! Combo: {Combo}");

        OnScoreComboChanged?.Invoke(Score, Combo);
    }

    /// <summary>
    /// Call when an enemy is actually destroyed.
    /// Awards score using the current combo but does NOT change the combo itself.
    /// Formula: scoreGain = baseScore + combo * 10
    /// </summary>
    public void RegisterEnemyDefeated(int baseScore)
    {
        if (IsGameOver) return;

        int scoreGain = baseScore + (Combo * 10);
        Score += scoreGain;

        Debug.Log($"Enemy defeated! Score: {Score}, Combo: {Combo}, Gain: {scoreGain}");

        OnScoreComboChanged?.Invoke(Score, Combo);
    }

    /// <summary>
    /// Backward-compatible wrapper used by single-hit enemies.
    /// Calls RegisterSuccessfulHit() then RegisterEnemyDefeated(baseScore)
    /// so Phase 1–5 behaviour is fully preserved without changing PlayerCombat.
    /// </summary>
    public void RegisterHit(int baseScore)
    {
        RegisterSuccessfulHit();
        RegisterEnemyDefeated(baseScore);
    }

    /// <summary>
    /// Called when the player attacks and finds no valid enemy on that side.
    /// Resets combo immediately and notifies the UI.
    /// </summary>
    public void RegisterMiss()
    {
        if (IsGameOver) return;

        Combo          = 0;
        hasActiveCombo = false;

        Debug.Log("Miss! Combo reset.");

        OnScoreComboChanged?.Invoke(Score, Combo);
    }

    /// <summary>
    /// Utility – zero out combo without logging.
    /// Can be called externally if other systems need to break the streak.
    /// </summary>
    public void ResetCombo()
    {
        Combo          = 0;
        hasActiveCombo = false;
    }

    /// <summary>
    /// Triggers game over. Safe to call multiple times – only the first call runs.
    /// Logs final score/combo and fires OnGameOverEvent for the UI.
    /// </summary>
    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;

        Debug.Log("Game Over");
        Debug.Log($"Final Score: {Score}");
        Debug.Log($"Final Combo: {Combo}");

        OnGameOverEvent?.Invoke(Score, Combo);
    }
}
