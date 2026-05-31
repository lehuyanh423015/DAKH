using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// PauseMenuManager – Handles pausing, unpausing, and Pause Menu UI interactions (Phase 22).
/// </summary>
public class PauseMenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Pause Panel UI object to show/hide when paused.")]
    private GameObject pausePanel;

    [SerializeField]
    [Tooltip("Name of the Main Menu scene to load when 'Main Menu' is clicked.")]
    private string mainMenuSceneName = "MainMenu";

    [SerializeField]
    [Tooltip("If true, logs actions in the console.")]
    private bool logActions = true;

    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent += HandleGameOver;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent -= HandleGameOver;
        }
    }

    private void HandleGameOver(int finalScore, int finalCombo)
    {
        // If game over happens while paused, ensure the pause panel is hidden
        // so it doesn't block the Game Over panel. Time remains frozen by GameManager.
        if (isPaused && pausePanel != null)
        {
            pausePanel.SetActive(false);
            isPaused = false;
        }
    }

    private void Start()
    {
        // Ensure the game starts unpaused and the panel is hidden.
        Time.timeScale = 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Toggle pause when ESC is pressed.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Do not allow pausing if the game is over.
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                if (logActions) Debug.Log("PauseMenuManager: Cannot pause, Game Over panel is active.");
                return;
            }

            TogglePause();
        }
    }

    /// <summary>
    /// Toggles the pause state between paused and unpaused.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    /// <summary>
    /// Pauses the game by setting Time.timeScale to 0 and showing the pause panel.
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        else if (logActions)
        {
            Debug.LogWarning("PauseMenuManager: PausePanel is not assigned!");
        }

        if (logActions) Debug.Log("PauseMenuManager: Game Paused.");
    }

    /// <summary>
    /// Resumes the game by setting Time.timeScale to 1 and hiding the pause panel.
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (logActions) Debug.Log("PauseMenuManager: Game Resumed.");
    }

    /// <summary>
    /// Reloads the current gameplay scene.
    /// </summary>
    public void RestartGame()
    {
        if (logActions) Debug.Log("PauseMenuManager: Restarting Game...");

        // Always restore time scale before loading a new scene.
        Time.timeScale = 1f;
        
        // Optional Audio Hook
        AudioManager.Instance?.PlayRestartClick();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    public void BackToMainMenu()
    {
        if (logActions) Debug.Log($"PauseMenuManager: Returning to {mainMenuSceneName}...");

        // Always restore time scale before loading a new scene.
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Exits the application.
    /// </summary>
    public void QuitGame()
    {
        // Always restore time scale, just in case.
        Time.timeScale = 1f;

#if UNITY_EDITOR
        if (logActions) Debug.Log("Quit requested. Application.Quit() does not close the Editor play mode.");
#else
        if (logActions) Debug.Log("PauseMenuManager: Quitting application...");
        Application.Quit();
#endif
    }
}
