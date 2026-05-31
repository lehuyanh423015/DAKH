# Extended Enemy Animation Guide

## 1. What Was Added
- **Enemy back animation**: All enemy types can now play a brief `back` animation on a non-lethal hit before returning to `run`.
- **Death animation for all types**: Every enemy type (Normal, Heavy, Switch, Pattern3Hit) now supports a `die` animation.
- **Death visual drift**: When an enemy is defeated, its visual sprite can optionally slide backward over a short duration to emphasize the hit impact.
- **Harmless dying enemies**: The moment an enemy's health reaches 0, its `Collider2D` is disabled and it ceases all movement and lane-blocking immediately, even as its visual death animation continues playing.
- **Non-lethal hit feedback**: Side-switching and heavy enemies will play the `back` animation seamlessly alongside their existing knockback/teleport logic.

---

## 2. Files Changed

| File | Change |
|------|--------|
| `Assets/Scripts/Enemies/EnemyAnimationController.cs` | Extended with `back` state support, `returnToRun` coroutine, and `DeathDriftRoutine` for visual-only slide. |
| `Assets/Scripts/Enemies/Enemy.cs` | Updated `TakeHit()` to trigger `PlayBack()`, added `GetDeathDriftDirection()` to pass physical impact direction to the animation controller. |

*(No changes were required in `PlayerCombat.cs` because the delayed-destroy logic added previously automatically handles the new extended animation system.)*

---

## 3. Important Design Explanations

**Root vs. Visual Separation:**
- **Enemy Root**: Maintains complete control over gameplay logic (`Enemy.cs`), the `Rigidbody2D`, and the `BoxCollider2D`. The root never plays animations.
- **Visual Child**: Handles all visual state (`Animator`, `SpriteRenderer`, `EnemyAnimationController`).
- **Death Drift**: When an enemy dies, the visual child slides locally (`transform.localPosition` changes). The enemy root object *does not move*. This ensures the gameplay state remains safe and static, while delivering dynamic visual polish.

---

## 4. Enemy Setup Checklist
For *each* enemy prefab (NormalEnemy, HeavyEnemy, SwitchEnemy, PatternEnemy3Hit), perform the following setup:

### Hierarchy
```text
[EnemyPrefab]                ← Gameplay Root
└── Visual                   ← Child Object
    ├── SpriteRenderer
    ├── Animator
    ├── VisualRoot
    └── EnemyAnimationController
```

### Inspector Setup
- [ ] Visual child exists and has all 4 required components.
- [ ] `VisualRoot.SpriteRenderer` is assigned.
- [ ] `VisualRoot.Animator` is assigned.
- [ ] Animator Controller has state **`run`** (Loop Time = true).
- [ ] Animator Controller has state **`back`** (Loop Time = false). *(Optional for 1-hit enemies)*
- [ ] Animator Controller has state **`die`** (Loop Time = false).
- [ ] `EnemyAnimationController.runStateName` = `"run"`.
- [ ] `EnemyAnimationController.backStateName` = `"back"`.
- [ ] `EnemyAnimationController.dieStateName` = `"die"`.
- [ ] `dieDuration` matches your die animation clip length.
- [ ] `useDeathDrift` is enabled (optional, but recommended).

---

## 5. How to Test

### HeavyEnemy
1. Let a HeavyEnemy spawn.
2. Hit it once. Confirm the **`back`** animation plays while the physical knockback pushes the enemy away.
3. Hit it again. Confirm the **`die`** animation plays. The enemy should freeze in place (or drift backward) and become harmless immediately.
4. Confirm the enemy disappears after the `dieDuration`.

### SwitchEnemy
1. Let a SwitchEnemy spawn.
2. Hit it once. Confirm the **`back`** animation plays *as* the enemy rapidly moves to the opposite side of the screen.
3. Once the side-switch completes, confirm it returns to the **`run`** animation as it approaches you again.
4. Hit the second side to defeat it. Confirm the **`die`** animation plays.

### PatternEnemy3Hit
1. Hit 1 → plays **`back`** + switches side.
2. Hit 2 → plays **`back`** + switches side.
3. Hit 3 → plays **`die`**.
4. Walk into the dying enemy to confirm it does not trigger a Game Over.

---

## 6. Tuning Notes

| Field | Effect |
|-------|--------|
| `backDuration` | How long the enemy stays in the `back` pose before automatically returning to `run`. Should roughly match the clip length (e.g., `0.18s`). |
| `dieDuration` | Must match the die clip length to ensure the enemy doesn't pop out of existence too early (e.g., `0.35s`). |
| `deathDriftDistance` | How far (in Unity units) the visual sprite slides backward upon death. |
| `deathDriftDuration` | How fast the slide happens. Should be less than or equal to `dieDuration`. |

---

## 7. Known Limitations
- Animations are purely visual. They do not dictate invincibility frames or attack timings.
- The `back` animation plays synchronously alongside physical side-switching movement. If the animation feels weird during the teleport, you can tweak `backDuration` to make it snap back to `run` faster.
- There are no animation events used; everything operates on code-controlled durations.
