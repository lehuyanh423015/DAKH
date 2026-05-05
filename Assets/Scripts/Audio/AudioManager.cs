using UnityEngine;

/// <summary>
/// AudioManager – Centralizes simple gameplay sound playback (Phase 16).
///
/// Responsibilities:
///   - Plays hit, miss, stun, game over, shield, and UI sounds via PlayOneShot.
///   - Auto-detects AudioSource if not assigned.
///   - Safe to use even if clips are missing (logs warnings optionally).
///
/// Scene setup:
///   - Create "AudioManager" empty GameObject in MainScene.
///   - Attach this script and an AudioSource (Play On Awake = false, Spatial Blend = 0).
///   - Assign clips in Inspector.
/// </summary>
public class AudioManager : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Singleton
    // ──────────────────────────────────────────────────────────────────────────
    public static AudioManager Instance { get; private set; }

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Audio Source")]
    [Tooltip("The AudioSource used to play sounds. Auto-detected if empty.")]
    [SerializeField] private AudioSource audioSource;

    [Tooltip("Global volume for all clips played by this manager.")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1.0f;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private AudioClip enemyDefeatedClip;
    [SerializeField] private AudioClip missClip;
    [SerializeField] private AudioClip stunClip;
    [SerializeField] private AudioClip gameOverClip;
    [SerializeField] private AudioClip shieldGainedClip;
    [SerializeField] private AudioClip shieldConsumedClip;
    [SerializeField] private AudioClip restartClickClip;

    [Header("Debug")]
    [Tooltip("If true, warns in the console when a requested clip is not assigned.")]
    [SerializeField] private bool logMissingClips = false;

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

        // Auto-detect or add AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D sound
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    public void PlayHit()            => PlayClip(hitClip, "Hit");
    public void PlayEnemyDefeated()  => PlayClip(enemyDefeatedClip, "Enemy Defeated");
    public void PlayMiss()           => PlayClip(missClip, "Miss");
    public void PlayStun()           => PlayClip(stunClip, "Stun");
    public void PlayGameOver()       => PlayClip(gameOverClip, "Game Over");
    public void PlayShieldGained()   => PlayClip(shieldGainedClip, "Shield Gained");
    public void PlayShieldConsumed() => PlayClip(shieldConsumedClip, "Shield Consumed");
    public void PlayRestartClick()   => PlayClip(restartClickClip, "Restart Click");

    // ──────────────────────────────────────────────────────────────────────────
    // Internal Helper
    // ──────────────────────────────────────────────────────────────────────────

    private void PlayClip(AudioClip clip, string clipName)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
        else if (logMissingClips)
        {
            Debug.LogWarning($"AudioManager: Cannot play '{clipName}'. Clip or AudioSource is missing.");
        }
    }
}
