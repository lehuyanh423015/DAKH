using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// GameUI – Manages all on-screen UI for the game.
///
/// Responsibilities:
///   - Displays live score and combo on screen.
///   - Shows the Game Over panel when the game ends.
///   - Shows the final score inside the Game Over panel.
///   - Provides a Restart button that reloads the current scene.
///
/// How it works (event-driven):
///   On Start, this script subscribes to two GameManager events:
///     - OnScoreComboChanged → refreshes scoreText and comboText.
///     - OnGameOverEvent     → shows the Game Over panel and final score.
///   On destroy/disable, it unsubscribes to prevent memory leaks.
///
/// Scene setup:
///   1. Create a Canvas in the scene (UI → Canvas).
///   2. Add child objects: ScoreText, ComboText, GameOverPanel.
///      Inside GameOverPanel: GameOverTitleText, FinalScoreText, RestartButton.
///   3. Create an empty GameObject named "GameUI" (or attach directly to Canvas).
///   4. Attach this script to it.
///   5. Drag all the UI objects into the matching slots in the Inspector.
///
/// TextMeshPro note:
///   This script uses TextMeshProUGUI (TMPro). If TMP is not yet imported,
///   go to Window → TextMeshPro → Import TMP Essential Resources.
///   If you prefer legacy UI Text, replace TextMeshProUGUI with Text and
///   remove the "using TMPro;" line.
/// </summary>
public class GameUI : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields  –  drag your UI objects into these slots
    // ──────────────────────────────────────────────────────────────────────────

    [Header("HUD")]
    [Tooltip("TextMeshPro text that shows the current score. Example: 'Score: 350'")]
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("TextMeshPro text that shows the current combo. Example: 'Combo: 3'")]
    [SerializeField] private TextMeshProUGUI comboText;

    [Tooltip("TextMeshPro text that shows the current shield status. Example: 'Shield: READY'")]
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("Game Over Panel")]
    [Tooltip("The panel that is hidden during play and shown when the game ends.")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("TextMeshPro text inside the Game Over panel. Shows 'Final Score: 350'.")]
    [SerializeField] private TextMeshProUGUI finalScoreText;

    [Tooltip("The Restart button inside the Game Over panel.")]
    [SerializeField] private Button restartButton;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Start()
    {
        // ── Subscribe to GameManager events ──────────────────────────────────
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreComboChanged += HandleScoreComboChanged;
            GameManager.Instance.OnComboShieldChanged += HandleComboShieldChanged;
            GameManager.Instance.OnGameOverEvent     += HandleGameOver;
        }
        else
        {
            Debug.LogWarning("GameUI: GameManager.Instance not found. Make sure GameManager exists in the scene.");
        }

        // ── Set up Restart button ────────────────────────────────────────────
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogWarning("GameUI: restartButton is not assigned in the Inspector.");
        }

        // ── Initial display ──────────────────────────────────────────────────
        // Show 0 score and 0 combo at the start of the game.
        UpdateScoreComboText(0, 0);

        if (GameManager.Instance != null)
        {
            UpdateComboShieldText(GameManager.Instance.ComboShields);
        }

        // Hide the Game Over panel until the game actually ends.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameUI: gameOverPanel is not assigned in the Inspector.");
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks (the scene may be destroyed
        // while the GameManager singleton lives on).
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnScoreComboChanged -= HandleScoreComboChanged;
            GameManager.Instance.OnComboShieldChanged -= HandleComboShieldChanged;
            GameManager.Instance.OnGameOverEvent     -= HandleGameOver;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Event handlers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by GameManager whenever score or combo changes (hit or miss).
    /// </summary>
    private void HandleScoreComboChanged(int score, int combo)
    {
        UpdateScoreComboText(score, combo);
    }

    /// <summary>
    /// Called by GameManager whenever the combo shield count changes.
    /// </summary>
    private void HandleComboShieldChanged(int comboShields)
    {
        UpdateComboShieldText(comboShields);
    }

    /// <summary>
    /// Called by GameManager exactly once when the game ends.
    /// Shows the Game Over panel and displays the final score.
    /// </summary>
    private void HandleGameOver(int finalScore, int finalCombo)
    {
        // Show the Game Over panel.
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        // Display the final score inside the panel.
        if (finalScoreText != null)
        {
            finalScoreText.text = "Final Score: " + finalScore;
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Updates scoreText and comboText with the provided values.
    /// Safe to call even if the references are not assigned yet.
    /// </summary>
    private void UpdateScoreComboText(int score, int combo)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }

        if (comboText != null)
        {
            comboText.text = "Combo: " + combo;
        }
    }

    /// <summary>
    /// Updates shieldText based on whether a shield is available.
    /// Safe to call even if the reference is not assigned yet.
    /// </summary>
    private void UpdateComboShieldText(int comboShields)
    {
        if (shieldText != null)
        {
            if (comboShields > 0)
            {
                shieldText.text = "Shield: READY";
            }
            else
            {
                shieldText.text = "Shield: -";
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Restart
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Reloads the current scene, effectively resetting the whole game.
    /// Score, combo, and game over state all reset because the scene is fresh.
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
