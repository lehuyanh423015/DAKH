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
public enum GameMode
{
    Normal,
    Demo
}

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

    /// <summary>Fired when a combo shield is gained or consumed. Parameter: (comboShields).</summary>
    public event System.Action<int> OnComboShieldChanged;

    /// <summary>Fired once when the game ends. Parameters: (finalScore, finalCombo).</summary>
    public event System.Action<int, int> OnGameOverEvent;

    /// <summary>Fired when persistent high score or high combo changes. Parameters: (highScore, highCombo).</summary>
    public event System.Action<int, int> OnHighScoreComboChanged;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields  (Phase 6 & Phase 25)
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Game Mode (Phase 25)")]
    [SerializeField] private GameMode gameMode = GameMode.Normal;


    [Tooltip("Seconds after the last successful hit before the combo expires. Recommended: 1.0.")]
    [SerializeField] private float comboWindowDuration = 1.0f;

    [Header("Combo Shield (Phase 15)")]
    [Tooltip("Number of consecutive hits required to earn a combo shield.")]
    [SerializeField] private int comboShieldThreshold = 10;

    [Tooltip("Maximum number of shields the player can hold at once.")]
    [SerializeField] private int maxComboShields = 1;

    [Tooltip("If true, a shielded miss resets combo to 0. If false, combo is maintained.")]
    [SerializeField] private bool resetComboOnShieldedMiss = true;

    [Header("Session Tracking (Phase 17)")]
    [Tooltip("Track and log the total session time.")]
    [SerializeField] private bool trackSessionTime = true;

    [Header("Phase 24 - Freeze Polish")]
    [Tooltip("If true, freezes time (Time.timeScale = 0) upon Game Over.")]
    [SerializeField] private bool freezeTimeOnGameOver = true;

    [Header("Debug")]
    [Tooltip("If true, logs hit, miss, and combo timeout events.")]
    [SerializeField] private bool logCombat = false;

    private const string HighScorePlayerPrefsKey = "HighScore";
    private const string HighComboPlayerPrefsKey = "HighCombo";

    // ──────────────────────────────────────────────────────────────────────────
    // State
    // ──────────────────────────────────────────────────────────────────────────

    public bool IsDemoMode => gameMode == GameMode.Demo;
    public GameMode CurrentGameMode => gameMode;

    /// <summary>True once GameOver() has been called.</summary>
    public bool IsGameOver { get; private set; } = false;

    /// <summary>Current accumulated score.</summary>
    public int Score { get; private set; } = 0;

    /// <summary>Current consecutive-hit streak.</summary>
    public int Combo { get; private set; } = 0;

    /// <summary>Best score saved on this machine.</summary>
    public int HighScore { get; private set; } = 0;

    /// <summary>Best combo saved on this machine.</summary>
    public int HighCombo { get; private set; } = 0;

    /// <summary>Exposes the combo window so UI or other systems can read it.</summary>
    public float ComboWindowDuration => comboWindowDuration;

    private int comboShields = 0;
    public int ComboShields => comboShields;
    public int ComboShieldThreshold => comboShieldThreshold;
    public bool HasComboShield => comboShields > 0;

    private float sessionTime = 0f;
    public float SessionTime => sessionTime;

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

        LoadHighStats();
    }

    private void Update()
    {
        // Only check timeout while a combo is active and game is still running.
        if (IsGameOver)          return;

        if (trackSessionTime)
        {
            sessionTime += Time.deltaTime;
        }

        if (!hasActiveCombo)     return;

        if (Time.time - lastSuccessfulHitTime > comboWindowDuration)
        {
            // Combo window elapsed – reset silently and notify UI once.
            Combo         = 0;
            hasActiveCombo = false;
            if (logCombat) Debug.Log("Combo expired.");
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

        if (logCombat) Debug.Log($"Hit registered! Combo: {Combo}");

        TryUpdateHighCombo();

        // Check for shield gain (Phase 15)
        if (Combo >= comboShieldThreshold && comboShields < maxComboShields)
        {
            comboShields++;
            if (logCombat) Debug.Log("Combo Shield gained!");
            AudioManager.Instance?.PlayShieldGained();
            OnComboShieldChanged?.Invoke(comboShields);
        }

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

        TryUpdateHighScore();

        if (logCombat) Debug.Log($"Enemy defeated! Score: {Score}, Combo: {Combo}, Gain: {scoreGain}");

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

        if (logCombat) Debug.Log("Miss! Combo reset.");

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

        if (IsDemoMode)
        {
            if (logCombat) Debug.Log("Demo Mode: Player hit by enemy. Resetting combo, ignoring GameOver.");
            ResetCombo();
            return;
        }

        IsGameOver = true;

        Debug.Log("Game Over");
        Debug.Log($"Final Score: {Score}");
        Debug.Log($"Final Combo: {Combo}");
        if (trackSessionTime)
        {
            Debug.Log($"Survival Time: {sessionTime:0.00}s");
        }
        Debug.Log($"High Score: {HighScore}");
        Debug.Log($"High Combo: {HighCombo}");

        comboShields = 0;
        OnComboShieldChanged?.Invoke(comboShields);

        AudioManager.Instance?.PlayGameOver();
        OnGameOverEvent?.Invoke(Score, Combo);

        if (freezeTimeOnGameOver)
        {
            Time.timeScale = 0f;
        }
    }

    /// <summary>
    /// Consumes a combo shield if one is available.
    /// Returns true if a shield was consumed, false otherwise.
    /// </summary>
    public bool TryConsumeComboShield()
    {
        if (IsGameOver || comboShields <= 0) return false;

        comboShields--;
        if (logCombat) Debug.Log("Combo Shield consumed!");
        OnComboShieldChanged?.Invoke(comboShields);
        return true;
    }

    private void LoadHighStats()
    {
        HighScore = PlayerPrefs.GetInt(HighScorePlayerPrefsKey, 0);
        HighCombo = PlayerPrefs.GetInt(HighComboPlayerPrefsKey, 0);
    }

    private void TryUpdateHighScore()
    {
        if (IsDemoMode) return;
        if (Score <= HighScore) return;

        HighScore = Score;
        PlayerPrefs.SetInt(HighScorePlayerPrefsKey, HighScore);
        PlayerPrefs.Save();
        OnHighScoreComboChanged?.Invoke(HighScore, HighCombo);
    }

    private void TryUpdateHighCombo()
    {
        if (IsDemoMode) return;
        if (Combo <= HighCombo) return;

        HighCombo = Combo;
        PlayerPrefs.SetInt(HighComboPlayerPrefsKey, HighCombo);
        PlayerPrefs.Save();
        OnHighScoreComboChanged?.Invoke(HighScore, HighCombo);
    }
}
