# Phase 12 Hotfix – SwitchEnemy Side-Switch Safety Fix

## 1. What Was Wrong

| Problem | Root cause |
|---------|-----------|
| **Instant Game Over after hit 1** | `OnTriggerEnter2D` fired while the enemy was crossing through the player's collider. |
| **Extra hit consumed during crossing** | A fast second input triggered `TakeHit()` again while the enemy was mid-animation, consuming health and sometimes triggering an extra side-switch. |
| **Target position sometimes overlapped player** | Target X was computed as `playerX ± sideSwitchDistance` with no account for the player or enemy collider widths, so landing could clip into the player's hitbox. |
| **Side enum updated too late** | `Side` was flipped at the **end** of the coroutine, so lane blocking and hit detection used the wrong side for the entire duration of the crossing. |

---

## 2. What Was Fixed

### `isSwitchingSide` flag
A new private boolean set to `true` for the entire crossing duration:
- **`OnTriggerEnter2D`**: if `isSwitchingSide == true`, skip the Game Over call.
- **`TakeHit()`**: if `isSwitchingSide == true`, ignore the hit silently (return `false`).

### Collider-aware safe target position
Old formula (unsafe):
```csharp
targetX = playerX + direction * sideSwitchDistance;
```

New formula (safe):
```csharp
safeDist = sideSwitchGap + playerHalfWidth + enemyHalfWidth;
targetX  = playerX ± safeDist;
```

- `sideSwitchGap` (Inspector field, default `1.2`) is the clear air gap between edges.
- `playerHalfWidth` = `playerCollider.bounds.extents.x` (fallback: sprite, then `0.5`).
- `enemyHalfWidth`  = `enemyCollider.bounds.extents.x` (fallback: sprite, then `0.5`).

With default values and typical collider sizes (~0.4 each), the landing X is roughly `playerX ± 2.0` — safely outside the player's collider.

### Side enum flipped **before** movement starts
The `Side` property now updates to `newSide` at the **top** of the coroutine, before any lerp frames. This means:
- Hit detection uses the new side from frame 1 of the crossing.
- Lane blocking on the new side is correct immediately.
- Enemies on the old side stop blocking against a SwitchEnemy that has already left.

### Helper methods added
```
GetHalfWidth()                          → this enemy's X extents
GetTargetHalfWidth()                    → player's X extents
CalculateSafeSwitchDistance()           → sideSwitchGap + both halfWidths
GetSafeSwitchTargetPosition(newSide)    → final world Vector3 for the crossing target
```

---

## 3. Inspector Checklist

### SwitchEnemy prefab
- [ ] `behaviorType` = `SwitchSideOnHit`
- [ ] `maxHealth` = `2`
- [ ] `scoreValue` = `250`
- [ ] `moveSpeed` = `2.5` (same as all other enemies)
- [ ] `sideSwitchGap` = `1.2`
- [ ] `sideSwitchDuration` = `0.08`
- [ ] `knockbackDistance` = `0.45` (unused for SwitchEnemy but kept consistent)
- [ ] `knockbackDuration` = `0.08` (unused)
- [ ] `enableLaneBlocking` = ✓
- [ ] `minSpacingFromFrontEnemy` = `0.8`
- [ ] `logPatternActions` = ✗ (enable to debug switch events)
- [ ] **Collider2D is present** — required for accurate half-width calculation.
- [ ] **Sprite color is distinct** (green / cyan / yellow).

### EnemySpawner
- [ ] `enemyPrefabs[0]` = NormalEnemy
- [ ] `enemyPrefabs[1]` = HeavyEnemy
- [ ] `enemyPrefabs[2]` = SwitchEnemy
- [ ] `Difficulty Manager` still assigned.
- [ ] `Player Transform` still assigned.

### Everything else (unchanged)
- [ ] `PlayerCombat → attackRange` still set.
- [ ] `GameUI` references intact.
- [ ] `Main Camera` has `CameraFollow` + `CameraShake`.
- [ ] `Player` has `PlayerMovement` + `PlayerCombat`.

---

## 4. How to Test

### Test A – SwitchEnemy from Left
1. Let SwitchEnemy approach from the **left**.
2. Attack **left** — combo+1, no score.
3. ✅ SwitchEnemy smoothly crosses to the **right** side.
4. ✅ **No Game Over** fires during the crossing.
5. ✅ A second A-press during crossing is ignored (no health change).
6. Attack **right** — combo+1, score +250+bonus, SwitchEnemy destroyed.

### Test B – SwitchEnemy from Right
1. Let SwitchEnemy approach from the **right**.
2. Attack **right** — combo+1, no score.
3. ✅ SwitchEnemy smoothly crosses to the **left** side.
4. ✅ No Game Over during crossing.
5. Attack **left** — SwitchEnemy destroyed, score awarded.

### Test C – Wrong side after switch
1. Hit SwitchEnemy (it crosses from Left to Right).
2. Attack **left** — no valid enemy on left → miss, stun fires.
3. Attack **right** — SwitchEnemy is hit and destroyed.

### Test D – Natural Game Over still works
1. Let SwitchEnemy reach the player **after** its switch and full landing.
2. ✅ Game Over fires normally — `isSwitchingSide` is false at that point.

### Test E – Collider bounds verification
Enable `logPatternActions` on the SwitchEnemy prefab. After hit 1, Console should show:
```
[SwitchEnemy(Clone)] Switching from Left → Right.
[SwitchEnemy(Clone)] Switched to Right. Position: 2.00
```
The landing X should be `> playerX` (for Right) with a safe gap visible in the Scene view.

---

## 5. Tuning Guide

| Field | Effect | Tune if... |
|-------|--------|-----------|
| `sideSwitchGap` | Clear air gap between edges after landing | Too close to player → increase; too far away → decrease |
| `sideSwitchDuration` | How long the crossing takes | Feels too slow → decrease (min 0.05); too snappy → increase |
| `minSpacingFromFrontEnemy` | Lane queue gap | Enemies overlap visually → increase |

---

## 6. Known Limitations

- Mid-switch hit window: because `Side` is flipped before the enemy crosses, a fast player could attack the new side before the enemy visually arrives. In practice the 0.08 s window is too short to notice but is a known edge case.
- No visual flash during crossing — purely positional movement.
- Collider bounds are sampled at switch-start; if the player has a trigger collider and a separate physics collider, `GetTargetHalfWidth` picks the first component found.
