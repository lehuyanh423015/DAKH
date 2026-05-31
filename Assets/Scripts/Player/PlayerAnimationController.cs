using System.Collections;
using UnityEngine;

/// <summary>
/// PlayerAnimationController – Drives the Player's Animator and SpriteRenderer.FlipX
/// based on gameplay events. Works with the Phase 19 Visual architecture.
///
/// Attach to Player/Visual (alongside VisualRoot) or to the Player root.
/// All references auto-detect from children if not assigned in the Inspector.
/// </summary>
public class PlayerAnimationController : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – references
    // ──────────────────────────────────────────────────────────────────────────

    [Header("References (auto-detected if empty)")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private VisualRoot visualRoot;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – sprite orientation
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Sprite Orientation")]
    [Tooltip("True  = sprite artwork faces/attacks right by default.\n" +
             "False = sprite artwork faces/attacks left by default.")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – timing
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Attack Timing")]
    [Tooltip("Seconds of inactivity after which the attack chain resets to attack1.")]
    [SerializeField] private float attackChainResetTime = 0.85f;

    [Tooltip("Seconds after an attack starts before the animator returns to idle. " +
             "Should be close to but not longer than the attack clip length.")]
    [SerializeField] private float attackReturnToIdleDelay = 0.35f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – Animator state names
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Animator State Names")]
    [SerializeField] private string idleStateName    = "idle";
    [SerializeField] private string attack1StateName = "attack1";
    [SerializeField] private string attack2StateName = "attack2";
    [SerializeField] private string attack3StateName = "attack3";
    [SerializeField] private string stunStateName    = "stun";

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – debug
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField] private bool logAnimation = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private int       attackChainIndex;
    private float     lastAttackAnimTime;
    private Coroutine returnToIdleRoutine;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Resolve VisualRoot first (prefers assigned, then child search).
        if (visualRoot == null)
        {
            visualRoot = GetComponentInChildren<VisualRoot>();
        }

        // Resolve Animator.
        if (animator == null)
        {
            if (visualRoot != null && visualRoot.Animator != null)
                animator = visualRoot.Animator;
            else
                animator = GetComponentInChildren<Animator>();
        }

        // Resolve SpriteRenderer.
        if (spriteRenderer == null)
        {
            if (visualRoot != null && visualRoot.SpriteRenderer != null)
                spriteRenderer = visualRoot.SpriteRenderer;
            else
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (logAnimation)
        {
            Debug.Log($"[PAC] Awake – Animator={animator}, SR={spriteRenderer}, VR={visualRoot}");
        }
    }

    private void Start()
    {
        PlayIdle();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Plays the next attack animation in the cycle (attack1 → attack2 → attack3 → attack1),
    /// flipping the sprite toward the attack direction.
    /// </summary>
    /// <param name="direction">-1 = left, 1 = right.</param>
    public void PlayAttack(int direction)
    {
        SetFacingDirection(direction);

        // Reset chain if too much time has passed since the last attack anim.
        if (Time.time - lastAttackAnimTime > attackChainResetTime)
        {
            attackChainIndex = 0;
        }

        string stateName = attackChainIndex switch
        {
            0 => attack1StateName,
            1 => attack2StateName,
            _ => string.IsNullOrEmpty(attack3StateName) ? attack1StateName : attack3StateName,
        };

        attackChainIndex = (attackChainIndex + 1) % 3;
        lastAttackAnimTime = Time.time;

        TryPlayState(stateName);

        if (logAnimation) Debug.Log($"[PAC] PlayAttack direction={direction} state={stateName}");

        // Schedule return to idle.
        if (returnToIdleRoutine != null) StopCoroutine(returnToIdleRoutine);
        returnToIdleRoutine = StartCoroutine(ReturnToIdleAfter(attackReturnToIdleDelay));
    }

    /// <summary>
    /// Plays the stun animation. Call this when stun actually begins (not on shielded miss).
    /// After the stun ends externally, call PlayIdle().
    /// </summary>
    public void PlayStun()
    {
        // Cancel any pending return-to-idle so idle doesn't interrupt the stun clip.
        if (returnToIdleRoutine != null)
        {
            StopCoroutine(returnToIdleRoutine);
            returnToIdleRoutine = null;
        }

        PlayState(stunStateName);

        if (logAnimation) Debug.Log("[PAC] PlayStun");
    }

    /// <summary>Returns to the idle animation.</summary>
    public void PlayIdle()
    {
        PlayState(idleStateName);
        if (logAnimation) Debug.Log("[PAC] PlayIdle");
    }

    /// <summary>
    /// Flips the sprite so it faces the given direction.
    /// direction &lt; 0 = left, direction &gt; 0 = right.
    /// </summary>
    public void SetFacingDirection(int direction)
    {
        if (spriteRenderer == null || direction == 0) return;

        bool wantsRight = direction > 0;
        // If the default artwork faces right: flip only when we want to face left.
        bool flipX = spriteFacesRightByDefault ? !wantsRight : wantsRight;
        spriteRenderer.flipX = flipX;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void PlayState(string stateName)
    {
        if (animator == null) return;
        animator.Play(stateName, 0, 0f);
    }

    /// <summary>
    /// Plays a state, logging a warning if the state doesn't exist in the controller.
    /// Falls back gracefully without crashing.
    /// </summary>
    private void TryPlayState(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName))
        {
            if (logAnimation) Debug.LogWarning("[PAC] Attempted to play an empty state name.");
            return;
        }
        animator.Play(stateName, 0, 0f);
    }

    private IEnumerator ReturnToIdleAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayIdle();
        returnToIdleRoutine = null;
    }
}
