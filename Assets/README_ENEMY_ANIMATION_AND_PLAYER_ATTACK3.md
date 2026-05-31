# Enemy Animation & Player Attack3 Integration

## 1. What Was Added
- **Player attack3**: The attack chain now cycles `attack1 → attack2 → attack3 → attack1`. If the player stops for longer than `attackChainResetTime`, the chain resets to `attack1`.
- **EnemyAnimationController**: A new script that plays `run` by default and `die` on defeat, then returns the die clip duration so the caller knows how long to wait before destroying the object.
- **Safe delayed destroy**: When a NormalEnemy (or any enemy with an `EnemyAnimationController`) is defeated, `PlayerCombat` now waits for the death animation to finish before calling `Destroy`. The enemy is made harmless instantly (collider disabled, `isDefeated = true`) so it cannot trigger Game Over or block lanes during the animation.
- **`MakeHarmless()`**: Called inside `Enemy.TakeHit()` the moment health hits 0. Disables the Collider2D and sets `isDefeated = true` immediately, before any coroutine or destroy delay.
- **`PlayDeathAnimationIfAvailable()`**: Public method on `Enemy` that delegates to `EnemyAnimationController` and returns the wait duration. Returns `0` when no controller is present (backward-compatible instant destroy).

---

## 2. Files Changed

| File | Change |
|------|--------|
| `Assets/Scripts/Player/PlayerAnimationController.cs` | Added `attack3StateName`, replaced bool with `attackChainIndex % 3`, added `TryPlayState` helper, updated defaults |
| `Assets/Scripts/Enemies/EnemyAnimationController.cs` | **Created** – run/die playback |
| `Assets/Scripts/Enemies/Enemy.cs` | Added `EnemyAnimationController` field, `isDefeated` flag, `MakeHarmless()`, `PlayDeathAnimationIfAvailable()` |
| `Assets/Scripts/Player/PlayerCombat.cs` | Replaced immediate `Destroy` with delay-aware destroy after death animation |

---

## 3. Player Setup Checklist

- [ ] `Player/Visual` Animator Controller has states: `idle`, `attack1`, `attack2`, `attack3`, `stun`
- [ ] `PlayerAnimationController.attack3StateName` = `"attack3"`
- [ ] `attackChainResetTime` ≈ `0.85`
- [ ] `attackReturnToIdleDelay` ≈ `0.35` (adjust to match your attack clip lengths)
- [ ] `spriteFacesRightByDefault` set correctly for your artwork
- [ ] FlipX still flips when attacking left vs right

---

## 4. NormalEnemy Setup Checklist

Recommended hierarchy:
```text
NormalEnemy                  ← Enemy.cs, BoxCollider2D, Rigidbody2D
└── Visual                   ← SpriteRenderer, Animator, VisualRoot, EnemyAnimationController
```

- [ ] `NormalEnemy/Visual` has `SpriteRenderer`
- [ ] `NormalEnemy/Visual` has `Animator` with a valid Animator Controller
- [ ] `NormalEnemy/Visual` has `VisualRoot`
- [ ] `NormalEnemy/Visual` has `EnemyAnimationController`
- [ ] Animator Controller has state `run` (Loop Time = **true**)
- [ ] Animator Controller has state `die` (Loop Time = **false**)
- [ ] `EnemyAnimationController.runStateName` = `"run"`
- [ ] `EnemyAnimationController.dieStateName` = `"die"`
- [ ] `EnemyAnimationController.dieDuration` ≈ length of your die clip (e.g. `0.35`)

> **Other enemy types** (Heavy, Switch, Pattern3Hit) do not need `EnemyAnimationController`. The code falls back gracefully to an immediate destroy when it is absent.

---

## 5. How to Test – Player attack3

1. Press attack once → **attack1** plays.
2. Press again quickly → **attack2** plays.
3. Press again quickly → **attack3** plays.
4. Press again quickly → **attack1** cycles back.
5. Wait > `attackChainResetTime` seconds, then press → resets to **attack1**.

---

## 6. How to Test – NormalEnemy Animation

1. Enter Play mode and let a NormalEnemy spawn.
2. Confirm the **run** animation plays while it approaches.
3. Hit the NormalEnemy to defeat it.
4. Confirm the **die** animation plays briefly before the object disappears.
5. During the die animation, walk into the enemy sprite — confirm it does **not** trigger Game Over.
6. Confirm the enemy does not appear to block the lane queue during die animation.

---

## 7. Known Limitations
- Only **NormalEnemy** has an `EnemyAnimationController` at this stage. HeavyEnemy, SwitchEnemy, and PatternEnemy3Hit still destroy immediately and can receive animation support in a future phase.
- `dieDuration` is manually tuned and must match the die clip length. If the clip is shorter than `dieDuration`, the object lingers; if longer, it disappears early.
- No animation events are used; timing remains fully code-controlled.
- No death animation for the player yet.

---

## 8. Tuning Notes

| Field | Recommended | Effect |
|-------|-------------|--------|
| `attackChainResetTime` | `0.85` | How quickly you must follow up to advance the chain |
| `attackReturnToIdleDelay` | `0.35` | How long the attack pose holds before returning to idle |
| `dieDuration` | Match clip length | How long the enemy lingers after defeat |
