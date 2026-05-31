using System.Collections;
using UnityEngine;

/// <summary>
/// EnemyAnimationController – Controls run / back / die animations on an enemy Visual child.
///
/// Attach to the enemy's Visual child (alongside VisualRoot).
/// All references auto-detect if not assigned in the Inspector.
///
/// Death drift: moves only the visual Transform locally so the enemy root's collider
/// stays fixed and harmless while the sprite slides backward visually.
/// </summary>
public class EnemyAnimationController : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – references
    // ──────────────────────────────────────────────────────────────────────────

    [Header("References (auto-detected if empty)")]
    [SerializeField] private Animator animator;
    [SerializeField] private VisualRoot visualRoot;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Sprite Orientation")]
    [Tooltip("True  = sprite artwork faces/attacks right by default.\n" +
             "False = sprite artwork faces/attacks left by default.")]
    [SerializeField] private bool spriteFacesRightByDefault = true;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – animation state names
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Animator State Names")]
    [SerializeField] private string runStateName  = "run";
    [SerializeField] private string backStateName = "back";
    [SerializeField] private string dieStateName  = "die";

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – timing
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Timing")]
    [Tooltip("How long the back animation plays before returning to run.")]
    [SerializeField] private float backDuration = 0.18f;

    [Tooltip("How long to wait after the die animation starts before the object is destroyed. " +
             "Should roughly match the die clip length.")]
    [SerializeField] private float dieDuration = 0.35f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – death drift
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Death Drift (visual-only, moves only the Visual child)")]
    [Tooltip("If true, the visual slides backward after death to sell the hit.")]
    [SerializeField] private bool useDeathDrift = true;

    [Tooltip("How far (in local units) the visual slides backward after death.")]
    [SerializeField] private float deathDriftDistance = 0.45f;

    [Tooltip("How long (seconds) the death drift slide takes.")]
    [SerializeField] private float deathDriftDuration = 0.22f;

    // ──────────────────────────────────────────────────────────────────────────
    // Inspector fields – debug
    // ──────────────────────────────────────────────────────────────────────────

    [Header("Debug")]
    [SerializeField] private bool logAnimation = false;

    // ──────────────────────────────────────────────────────────────────────────
    // Private state
    // ──────────────────────────────────────────────────────────────────────────

    private Coroutine returnToRunRoutine;
    private Coroutine deathDriftRoutine;
    private Transform visualTransform;

    // ──────────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Resolve VisualRoot.
        if (visualRoot == null)
            visualRoot = GetComponentInChildren<VisualRoot>();

        // Resolve Animator.
        if (animator == null)
        {
            if (visualRoot != null && visualRoot.Animator != null)
                animator = visualRoot.Animator;
            else
                animator = GetComponent<Animator>();
        }

        // Resolve SpriteRenderer.
        if (spriteRenderer == null)
        {
            if (visualRoot != null && visualRoot.SpriteRenderer != null)
                spriteRenderer = visualRoot.SpriteRenderer;
            else
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Resolve the visual Transform to move during death drift.
        if (visualRoot != null)
            visualTransform = visualRoot.transform;
        else if (animator != null)
            visualTransform = animator.transform;
        else
            visualTransform = transform;
    }

    private void Start()
    {
        PlayRun();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Public API
    // ──────────────────────────────────────────────────────────────────────────

    /// <summary>Plays the run (looping movement) animation.</summary>
    public void PlayRun()
    {
        TryPlayState(runStateName);
        if (logAnimation) Debug.Log($"[EAC] PlayRun – {gameObject.name}");
    }

    /// <summary>
    /// Plays the back (hit reaction) animation then automatically returns to run.
    /// Safe to call even if the back state does not exist in the Animator Controller.
    /// </summary>
    public void PlayBack()
    {
        if (string.IsNullOrEmpty(backStateName)) return;

        if (returnToRunRoutine != null)
            StopCoroutine(returnToRunRoutine);

        TryPlayState(backStateName);
        if (logAnimation) Debug.Log($"[EAC] PlayBack – {gameObject.name}");

        returnToRunRoutine = StartCoroutine(ReturnToRunAfter(backDuration));
    }

    /// <summary>
    /// Plays the die animation and starts an optional death drift.
    /// Returns the delay (seconds) the caller should wait before destroying the object.
    /// </summary>
    /// <param name="deathDirection">-1 = drift left, 1 = drift right, 0 = no drift.</param>
    public float PlayDieAndGetDuration(int deathDirection)
    {
        // Cancel any pending back → run return.
        if (returnToRunRoutine != null)
        {
            StopCoroutine(returnToRunRoutine);
            returnToRunRoutine = null;
        }

        TryPlayState(dieStateName);
        if (logAnimation) Debug.Log($"[EAC] PlayDie – {gameObject.name} dir={deathDirection}");

        // Start the visual death drift (only moves the visual child locally).
        if (useDeathDrift && deathDirection != 0)
        {
            if (deathDriftRoutine != null)
                StopCoroutine(deathDriftRoutine);
            deathDriftRoutine = StartCoroutine(DeathDriftRoutine(deathDirection));
        }

        return dieDuration;
    }

    /// <summary>Overload without direction – uses no drift.</summary>
    public float PlayDieAndGetDuration()
    {
        return PlayDieAndGetDuration(0);
    }

    /// <summary>
    /// Flips the sprite so it faces the given direction.
    /// direction < 0 = left, direction > 0 = right.
    /// </summary>
    public void SetFacingDirection(int direction)
    {
        if (spriteRenderer == null || direction == 0) return;

        bool wantsRight = direction > 0;
        bool flipX = spriteFacesRightByDefault ? !wantsRight : wantsRight;
        spriteRenderer.flipX = flipX;

        if (logAnimation) Debug.Log($"[EAC] SetFacingDirection {direction} -> flipX: {flipX}");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Coroutines
    // ──────────────────────────────────────────────────────────────────────────

    private IEnumerator ReturnToRunAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        PlayRun();
        returnToRunRoutine = null;
    }

    /// <summary>
    /// Slides only the visual child's LOCAL position to sell the death hit.
    /// The root object (and its collider) does not move.
    /// </summary>
    private IEnumerator DeathDriftRoutine(int direction)
    {
        if (visualTransform == null) yield break;

        Vector3 startLocal  = visualTransform.localPosition;
        Vector3 targetLocal = startLocal + Vector3.right * direction * deathDriftDistance;

        float elapsed = 0f;
        while (elapsed < deathDriftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / deathDriftDuration);
            visualTransform.localPosition = Vector3.Lerp(startLocal, targetLocal, t);
            yield return null;
        }

        visualTransform.localPosition = targetLocal;
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────────────

    private void TryPlayState(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName)) return;
        animator.Play(stateName, 0, 0f);
    }
}
