using UnityEngine;

/// <summary>
/// VisualRoot – Identifies the child object containing the visual representation (Phase 19).
///
/// Purpose:
///   Separates gameplay logic/colliders on the root object from the visual 
///   sprites/animations on a child object. This makes it easy to swap out 
///   placeholder art with final animated assets later without breaking hitboxes.
/// </summary>
public class VisualRoot : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;

    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Animator Animator => animator;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }
}
