using UnityEngine;

/// <summary>
/// Scene-local looping background music controller.
/// Use one instance per scene for menu or gameplay music.
/// </summary>
public class SceneMusicController : MonoBehaviour
{
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool restartFromBeginningOnStart = true;
    [SerializeField] private bool keepPlayingOnGameOver = true;
    [SerializeField] private bool pauseOnGamePause = false;
    [SerializeField] private float volume = 0.6f;
    [SerializeField] private bool logMusic = false;

    [Header("Game Over (Phase 26 Polish)")]
    [SerializeField] private AudioClip gameOverMusicClip;
    [SerializeField] private bool playGameOverMusicOnGameOver = true;
    [SerializeField] private bool stopGameplayMusicOnGameOver = true;
    [SerializeField] private bool gameOverMusicLoops = false;
    [SerializeField] private float gameOverMusicVolume = 0.75f;

    private bool wasPausedByPauseState;
    private bool wasPausedByGameOver;
    private bool hasPlayedGameOverMusic;

    private void Awake()
    {
        EnsureMusicSource();
        ConfigureMusicSource();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent += HandleGameOverEvent;
        }

        if (playOnStart && musicClip != null)
        {
            if (restartFromBeginningOnStart)
            {
                musicSource.time = 0f;
            }

            musicSource.Play();
            if (logMusic) Debug.Log($"SceneMusicController: Playing '{musicClip.name}'.");
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOverEvent -= HandleGameOverEvent;
        }
    }

    private void Update()
    {
        if (musicSource == null) return;

        bool hasGameManager = GameManager.Instance != null;
        bool isGameOver = hasGameManager && GameManager.Instance.IsGameOver;

        if (isGameOver)
        {
            HandleGameOverMusic();
            return;
        }

        if (wasPausedByGameOver && keepPlayingOnGameOver == false)
        {
            wasPausedByGameOver = false;
        }

        if (pauseOnGamePause)
        {
            HandlePauseStateMusic();
        }
    }

    public void PlayFromBeginning()
    {
        EnsureMusicSource();
        ConfigureMusicSource();

        if (musicClip == null) return;

        musicSource.time = 0f;
        musicSource.Play();
        wasPausedByPauseState = false;
        wasPausedByGameOver = false;
        hasPlayedGameOverMusic = false;
    }

    public void StopMusic()
    {
        if (musicSource == null) return;

        musicSource.Stop();
        wasPausedByPauseState = false;
        wasPausedByGameOver = false;
    }

    public void PauseMusic()
    {
        if (musicSource == null) return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null) return;

        musicSource.UnPause();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);

        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    private void EnsureMusicSource()
    {
        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void ConfigureMusicSource()
    {
        if (musicSource == null) return;

        musicSource.playOnAwake = false;
        musicSource.loop = loop;
        musicSource.volume = volume;
        musicSource.spatialBlend = 0f;

        if (musicClip != null)
        {
            musicSource.clip = musicClip;
        }
    }

    private void HandlePauseStateMusic()
    {
        bool gamePaused = Time.timeScale == 0f;

        if (gamePaused && musicSource.isPlaying)
        {
            musicSource.Pause();
            wasPausedByPauseState = true;
            if (logMusic) Debug.Log("SceneMusicController: Music paused by pause state.");
        }
        else if (!gamePaused && wasPausedByPauseState)
        {
            musicSource.UnPause();
            wasPausedByPauseState = false;
            if (logMusic) Debug.Log("SceneMusicController: Music resumed after pause state.");
        }
    }

    private void HandleGameOverMusic()
    {
        if (!hasPlayedGameOverMusic)
        {
            hasPlayedGameOverMusic = true;

            if (playGameOverMusicOnGameOver && gameOverMusicClip != null)
            {
                if (stopGameplayMusicOnGameOver)
                {
                    musicSource.Stop();
                }

                musicSource.clip = gameOverMusicClip;
                musicSource.loop = gameOverMusicLoops;
                musicSource.volume = gameOverMusicVolume;
                musicSource.time = 0f;
                musicSource.Play();

                if (logMusic) Debug.Log($"SceneMusicController: Playing Game Over music '{gameOverMusicClip.name}'.");
                return; // We have taken over the audio source
            }
        }

        // Only apply pause fallback if we aren't already playing dedicated Game Over music
        if (gameOverMusicClip == null || !playGameOverMusicOnGameOver)
        {
            if (keepPlayingOnGameOver)
            {
                return;
            }

            if (musicSource.isPlaying)
            {
                musicSource.Pause();
                wasPausedByGameOver = true;
                if (logMusic) Debug.Log("SceneMusicController: Music paused by game over.");
            }
        }
    }

    private void HandleGameOverEvent(int finalScore, int finalCombo)
    {
        if (musicSource == null)
        {
            EnsureMusicSource();
            ConfigureMusicSource();
        }

        HandleGameOverMusic();
    }
}
