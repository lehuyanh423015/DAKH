using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenuManager – Handles Main Menu interactions (Phase 21).
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Name of the gameplay scene to load when Play is clicked.")]
    private string gameplaySceneName = "SampleScene";

    [SerializeField]
    [Tooltip("If true, logs actions in the console.")]
    private bool logActions = true;

    /// <summary>
    /// Loads the gameplay scene. Hook this up to the Play Button's OnClick event.
    /// </summary>
    public void PlayGame()
    {
        if (string.IsNullOrEmpty(gameplaySceneName))
        {
            if (logActions) Debug.LogWarning("MainMenuManager: Gameplay Scene Name is empty!");
            return;
        }

        if (logActions) Debug.Log($"MainMenuManager: Loading scene '{gameplaySceneName}'...");
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Exits the application. Hook this up to the Quit Button's OnClick event.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        if (logActions) Debug.Log("Quit requested. Application.Quit() does not close the Editor play mode.");
#else
        if (logActions) Debug.Log("MainMenuManager: Quitting application...");
        Application.Quit();
#endif
    }
}
