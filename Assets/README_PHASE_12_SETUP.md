# Phase 12 – SwitchEnemy (2-Hit Side-Switch Pattern) Setup Guide

## 1. What Was Added in Phase 12

| Feature | Details |
|---------|---------|
| **SwitchEnemy** | A new 2-hit enemy that crosses to the opposite side after the first hit. |
| **EnemyBehaviorType enum** | `Normal` (knockback) or `SwitchSideOnHit` (side crossing). Serialized per prefab. |
| **Side-switch routine** | Smooth lerp to `playerX ± sideSwitchDistance`; `Side` property flips at the end. |
| **Pattern-based difficulty** | SwitchEnemy is harder because it changes which direction the player must attack — not because it moves faster. |
| **Lane blocking compatibility** | Lane blocking is suppressed during the crossing window and resumes on the new side automatically. |

---

## 2. Files Created / Changed

### `Enemy.cs` — **updated**
New enum (Phase 12):
```csharp
public enum EnemyBehaviorType { Normal, SwitchSideOnHit }
```

New serialized fields:
- `EnemyBehaviorType behaviorType = Normal` — controls non-lethal-hit reaction.
- `float sideSwitchDistance = 1.2f` — distance from player on opposite side after switch.
- `float sideSwitchDuration = 0.12f` — seconds the crossing takes.
- `bool logPatternActions = false` — optional debug logging.

New private runtime fields:
- `float ignoreLaneBlockingUntil` — suppresses lane blocking during side-switch.

`TakeHit()` change:
- If `behaviorType == SwitchSideOnHit` → calls `TriggerSideSwitch()`.
- If `behaviorType == Normal` → calls `TriggerKnockback()` (unchanged).

New `SideSwitchRoutine()`:
1. Sets `isKnockedBack = true` (pauses movement).
2. Sets `ignoreLaneBlockingUntil = Time.time + sideSwitchDuration + 0.05f`.
3. Computes target X = `playerX ± sideSwitchDistance` on the opposite side.
4. Smooth-step lerps over `sideSwitchDuration`.
5. Flips `Side` property at the end.
6. Clears `isKnockedBack`.

`Update()` change:
- Lane blocking is skipped when `Time.time < ignoreLaneBlockingUntil`.

New public property: `public EnemyBehaviorType BehaviorType => behaviorType;`

### `PlayerCombat.cs` — **no changes**
Hit detection uses `enemyX < playerX` (left) and `enemyX > playerX` (right). After the side-switch the enemy physically crosses the player, so it is automatically classifiable on the new side. No code change required.

### `EnemySpawner.cs` — **no changes**
`enemyPrefabs[]` already supports multiple prefabs. Add SwitchEnemy manually.

### `GameManager.cs`, `GameUI.cs`, `DifficultyManager.cs`, `CameraFollow.cs`, `CameraShake.cs` — **no changes**

---

## 3. Important Design Explanation

### SwitchEnemy is pattern-based, not speed-based

| Enemy | HP | Score | Reaction to non-lethal hit | Speed |
|-------|----|-------|---------------------------|-------|
| NormalEnemy | 1 | 100 | — (dies in one hit) | Global |
| HeavyEnemy | 2 | 200 | Knockback (~0.45 units) | Global |
| SwitchEnemy | 2 | 250 | Crosses to opposite side | Global |

All three enemies receive the **same runtime speed** from `DifficultyManager`. The SwitchEnemy challenge is purely cognitive: the player must track which side to attack next.

### How side validation works

`PlayerCombat.TryAttack` checks:
```csharp
bool isOnCorrectSide =
    (targetSide == Left  && enemyX < playerX) ||
    (targetSide == Right && enemyX > playerX);
```
After the side-switch, the enemy's world X is now on the opposite side of the player, so it becomes valid on the new attack direction automatically. No special-casing is needed.

### Combo and score flow
- Hit 1 (correct side) → `RegisterSuccessfulHit()` (combo++) → `TakeHit()` returns false → side-switch starts. No score.
- Hit 2 (opposite side) → `RegisterSuccessfulHit()` (combo++) → `TakeHit()` returns true → `RegisterEnemyDefeated(250)` (score+). Destroy.

---

## 4. SwitchEnemy Prefab Setup

**Steps:**
1. Duplicate the **HeavyEnemy** prefab in `Assets/Prefabs/`.
2. Rename it **SwitchEnemy**.
3. In the Inspector, set:

| Field | Value |
|-------|-------|
| `maxHealth` | `2` |
| `scoreValue` | `250` |
| `moveSpeed` | `2.5` ← keep equal to NormalEnemy/HeavyEnemy |
| `behaviorType` | `SwitchSideOnHit` |
| `sideSwitchDistance` | `1.2` |
| `sideSwitchDuration` | `0.12` |
| `knockbackDistance` | `0.45` (unused by SwitchEnemy, kept for consistency) |
| `knockbackDuration` | `0.08` (unused) |
| `enableLaneBlocking` | ✓ |
| `minSpacingFromFrontEnemy` | `0.8` |
| `logPatternActions` | ✗ (enable only for debugging) |

4. Change sprite color to **green / cyan / yellow** — must be visually distinct from red (Normal) and purple (Heavy).
5. Set scale to `(0.8, 0.8, 1)`.

---

## 5. Inspector Checklist

### SwitchEnemy prefab
- [ ] `Enemy.cs` is attached.
- [ ] `maxHealth = 2`
- [ ] `scoreValue = 250`
- [ ] `moveSpeed = 2.5`
- [ ] `behaviorType = SwitchSideOnHit`
- [ ] `sideSwitchDistance = 1.2`
- [ ] `sideSwitchDuration = 0.12`
- [ ] `enableLaneBlocking` = ✓
- [ ] `minSpacingFromFrontEnemy = 0.8`
- [ ] `logPatternActions` = ✗
- [ ] Sprite color is visually distinct (green / cyan / yellow).
- [ ] Scale = `(0.8, 0.8, 1)`.

### EnemySpawner
- [ ] `enemyPrefabs` array size = 3.
- [ ] `enemyPrefabs[0]` = NormalEnemy.
- [ ] `enemyPrefabs[1]` = HeavyEnemy.
- [ ] `enemyPrefabs[2]` = SwitchEnemy.
- [ ] `Player Transform` still assigned.
- [ ] `Difficulty Manager` still assigned.

### Everything else (unchanged)
- [ ] `PlayerCombat → attackRange` still set (~2.0).
- [ ] `GameUI` references still assigned.
- [ ] `Main Camera` has `CameraFollow` + `CameraShake`.
- [ ] `Player` has `PlayerMovement` + `PlayerCombat`.

---

## 6. How to Test – SwitchEnemy Core Pattern

1. Temporarily set `enemyPrefabs` to only SwitchEnemy (or wait for one to spawn).
2. Let SwitchEnemy approach from the **left**.
3. Attack **left** when it enters range.
4. ✅ Console: `Hit registered! Combo: N`
5. ✅ SwitchEnemy is **NOT** destroyed. Score does not change.
6. ✅ SwitchEnemy smoothly crosses to the **right** side.
7. Attack **right**.
8. ✅ Console: `Hit registered! Combo: N+1`
9. ✅ Console: `Enemy defeated! Score: ..., Gain: 250 + bonus`
10. ✅ SwitchEnemy is destroyed.

**Enable `logPatternActions`** on the prefab to see:
```
[SwitchEnemy] SwitchEnemy switching from Left to Right.
[SwitchEnemy] SwitchEnemy switched to Right.
```

---

## 7. How to Test – Wrong-Side Miss

1. Hit SwitchEnemy once (it switches from Left to Right).
2. Press **A** (attack left) — SwitchEnemy is now on the right side.
3. ✅ No valid enemy on the left → registers as **miss** (if no other left-side enemy exists).
4. ✅ Combo resets. Stun fires.
5. Press **D** (attack right) → SwitchEnemy is hit again and defeated.

---

## 8. How to Test – Lane Blocking During Side-Switch

1. Let two enemies approach from the same side (e.g. SwitchEnemy in front, NormalEnemy behind).
2. Hit SwitchEnemy — it starts crossing to the opposite side.
3. ✅ SwitchEnemy's lane blocking is suppressed during crossing (it slides through).
4. ✅ NormalEnemy resumes moving toward player as SwitchEnemy leaves the lane.
5. ✅ After SwitchEnemy lands on the new side, it participates in lane blocking there.

---

## 9. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| NormalEnemy | Dies in 1 hit; combo+1; score increases |
| HeavyEnemy | Hit 1: knockback; hit 2: destroyed; score increases |
| SwitchEnemy | Hit 1: crosses side; hit 2: destroyed; score increases |
| Lane blocking | Same-side enemies queue; auto-resume on front death |
| Player shift | A/D shifts player within ±2.5 |
| Camera follow | Lags behind player X; stays within ±1.5 |
| Camera shake | Miss and game-over shake unchanged |
| Combo on every hit | All hit types (lethal, non-lethal, switch) count |
| Combo timeout | Resets after `comboWindowDuration` |
| Score | Only on defeat |
| Miss stun | Stun, tint, camera shake |
| Attack indicators | Flash on every key press |
| Hit effect | Lethal and non-lethal |
| Difficulty scaling | Speed and interval increase over time |
| Game Over panel | Appears on enemy touch |
| Restart | Scene reloads; all state resets |

---

## 10. Known Limitations

- ❌ SwitchEnemy mid-switch hit: if the player attacks during the crossing animation, the side check (`enemyX < playerX` / `enemyX > playerX`) determines validity based on current position. This means a brief window exists where the enemy could be hit from either side as it crosses. This is acceptable for a prototype.
- ❌ No 3-hit pattern enemy yet.
- ❌ No visual flash or particle during the side-switch — purely positional movement.
- ❌ Spawn selection is simple equal-chance random; no weights yet.
- ❌ Lane blocking during side-switch is simplified (time-based suppression, not full awareness).
- ❌ No combo shield / miss forgiveness yet.

---

## 11. Next Phase – Phase 13 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **3-hit pattern enemy** | Add a `TripleEnemy` with `maxHealth = 3`. Pattern: hit Left → knockback; hit Right → knockback; hit Left → defeat. Implement via a pattern array `SpawnSide[] hitPattern` in `Enemy`. |
| **General pattern array** | Replace `behaviorType` enum with a `hitPattern[]` array that defines which side each hit must come from and what happens after each non-lethal hit. |
| **Combo shield** | Forgive the first miss per combo streak. Add `bool comboShieldActive` to `GameManager`; reset it after each use. |
| **Spawn weights** | Add `float[] spawnWeights` parallel to `enemyPrefabs[]` in `EnemySpawner`. Use weighted random selection. |
| **HP color tint** | On non-lethal hit, tint enemy to orange/yellow using `SpriteRenderer.color` to show it has taken damage. |
