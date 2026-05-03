using UnityEngine;

/// <summary>
/// TemporaryEffect – Self-destructs after a configurable lifetime.
///
/// Usage:
///   1. Create a small GameObject (e.g. a white/yellow square sprite).
///   2. Attach this script to it.
///   3. Save as a prefab: Assets/Prefabs/HitEffect.prefab
///   4. Drag that prefab into the "Hit Effect Prefab" slot on PlayerCombat.
///
/// When PlayerCombat instantiates this prefab at an enemy's position,
/// it will appear for "lifetime" seconds then automatically destroy itself.
///
/// Inspector recommended value:
///   lifetime : 0.15 – 0.20 seconds
/// </summary>
public class TemporaryEffect : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How many seconds this object lives before being destroyed. Recommended: 0.15–0.20.")]
    private float lifetime = 0.15f;

    private void Start()
    {
        // Schedule destruction immediately when the object is created.
        Destroy(gameObject, lifetime);
    }
}
