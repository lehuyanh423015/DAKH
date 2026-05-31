# Player Animation Integration Guide

## 1. What Was Added
- **PlayerAnimationController**: A new optional script that drives the `Animator` and `SpriteRenderer.flipX` on the `Player/Visual` child object based on gameplay events.
- **Alternating Attack Animations**: `attack1` and `attack2` alternate on consecutive attacks chained within `attackChainResetTime` seconds, giving combat visual variety.
- **FlipX Direction Handling**: The player sprite automatically flips to face the direction of each attack.
- **Stun Animation Integration**: `stun` plays when a real miss penalty begins; it is skipped on a shielded miss (no stun penalty = no stun animation).
- **Idle Return**: After each attack finishes, the animator returns to `idle` after a short delay. After a stun ends, `idle` resumes automatically.

---

## 2. Files Created / Changed

| File | Action |
|------|--------|
| `Assets/Scripts/Player/PlayerAnimationController.cs` | **Created** |
| `Assets/Scripts/Player/PlayerCombat.cs` | **Updated** – wired animation calls |

`VisualRoot.cs` was **not changed** – it already exposes `Animator` and `SpriteRenderer`.

---

## 3. Inspector Checklist

### Player/Visual Object
- [ ] Has `SpriteRenderer`
- [ ] Has `Animator` with a valid Animator Controller
- [ ] Has `VisualRoot.cs`
- [ ] Has `PlayerAnimationController.cs` *(or it can be on the Player root)*

### PlayerAnimationController Settings
- [ ] **Animator** assigned *(or auto-detected)*
- [ ] **SpriteRenderer** assigned *(or auto-detected)*
- [ ] **spriteFacesRightByDefault** is `true` if sprite artwork points right, `false` if it points left
- [ ] **idleStateName** matches exactly the `idle` state name in your Animator Controller
- [ ] **attack1StateName** = `attack1`
- [ ] **attack2StateName** = `attack2`
- [ ] **stunStateName** = `stun`

### PlayerCombat Settings
- [ ] **Player Animation Controller** slot is assigned, OR leave it empty and rely on auto-detection via `GetComponentInChildren`

---

## 4. Animator Controller Setup
Your Animator Controller attached to `Player/Visual` must contain four states named **exactly**:

| State | Loop | Notes |
|-------|------|-------|
| `idle` | Yes | Default entry state |
| `attack1` | No | Plays on odd attacks |
| `attack2` | No | Plays on even attacks |
| `stun` | Optional | Loops if stun feels long, or plays once |

> **No transitions are required.** `PlayerAnimationController` uses `animator.Play(name, 0, 0f)` to jump directly to any state, bypassing the transition graph entirely. This keeps the Animator Controller minimal.

---

## 5. How to Test

1. Press **Play** in the gameplay scene.
2. **Idle** – confirm the `idle` animation loops at the start.
3. **attack1** – press A or D once. The player should snap into the attack1 pose and return to idle.
4. **attack2** – press the opposite direction (or same) quickly. The second attack should use `attack2`.
5. **Chain reset** – wait ~0.75 s without attacking, then attack again. Should use `attack1` again.
6. **FlipX** – press A (left attack). Sprite should face left. Press D (right attack). Sprite should face right.
7. **Stun** – miss an attack without a shield. Confirm `stun` animation plays and persists for the stun duration, then `idle` resumes.
8. **Shielded miss** – reach 10-combo shield, then miss. Confirm `stun` does **not** play (no real stun penalty).
9. **Gameplay unchanged** – combo, score, camera shake, lane blocking, enemy types, and restart should all work identically.

---

## 6. Tuning Notes

| Field | Effect |
|-------|--------|
| `attackChainResetTime` | Seconds of inactivity before the attack chain resets back to `attack1`. Default 0.75 s. |
| `attackReturnToIdleDelay` | How long to wait (in seconds) after an attack before returning to idle. Set close to—but not longer than—the attack clip duration. Default 0.18 s. |
| `spriteFacesRightByDefault` | Flip this if your sprite artwork is left-facing by default. |

---

## 7. Known Limitations
- Animation is **visual only** – hit detection timing is fully code-controlled and is not affected by animation clip length.
- No animation events are used.
- No death/game-over animation yet.
- No enemy animations yet.
- Separate left/right sprite sheets are not needed; `SpriteRenderer.flipX` handles mirroring.
