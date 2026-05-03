# Phase 8 – Multiple Enemy Spawning & Knockback Tuning Setup Guide

## 1. What Was Added in Phase 8

| Feature | Details |
|---------|---------|
| **Multi-prefab spawning** | `EnemySpawner` now has an `enemyPrefabs[]` array. Each spawn randomly picks one entry. |
| **NormalEnemy + HeavyEnemy mixing** | Both prefab variants can coexist; selection is equal-chance random. |
| **Consistent runtime speed** | All spawned enemies receive the same `DifficultyManager.CurrentEnemySpeed`. HeavyEnemy is **not** slower — it is harder because it requires 2 hits. |
| **Shorter knockback defaults** | `knockbackDistance` changed from `1.0` → `0.45`; `knockbackDuration` from `0.12` → `0.08` so the second-hit timing window feels natural. |
| **Null-safe prefab fallback** | If `enemyPrefabs` is empty or all-null, the single `enemyPrefab` slot is used as before. |
| **Debug spawn logging** | Optional `logSpawnedEnemyType` flag prints which prefab was chosen (default: off). |

---

## 2. Files Created / Changed

### `EnemySpawner.cs` — **updated**
- New `[SerializeField] private GameObject[] enemyPrefabs;` array (Phase 8 primary).
- Existing `[SerializeField] private GameObject enemyPrefab;` kept as fallback (Phase 1).
- New `PickPrefab()` helper: randomly selects from valid (non-null) `enemyPrefabs` entries; falls back to single slot; logs warning if neither is set.
- `SpawnEnemy()` now calls `PickPrefab()` and applies the same global speed to all types.
- New `[SerializeField] private bool logSpawnedEnemyType = false;` debug field.
- All Phase 1/5 behaviour (interval, side, DifficultyManager, game-over stop) preserved.

### `Enemy.cs` — **updated** (defaults + comments only)
- `knockbackDistance` default: `1.0f` → `0.45f`
- `knockbackDuration` default: `0.12f` → `0.08f`
- Tooltip and doc-comment updated to reflect Phase 8 tuning rationale.
- **No logic changes.** All Phase 7 methods unchanged.

### `PlayerCombat.cs` — **no changes**
### `GameManager.cs` — **no changes**
### `GameUI.cs` — **no changes**
### `DifficultyManager.cs` — **no changes**

> **⚠️ IMPORTANT — Unity serialization:**
> Existing prefabs that were configured in Phase 7 will retain their old Inspector values
> (`knockbackDistance = 1.0`, `knockbackDuration = 0.12`) because Unity serializes per-prefab.
> You **must** manually update both values in the Inspector on each prefab (see checklist below).

---

## 3. Important Design Explanation

### Why enemy types must share the same movement speed

Speed scaling is handled globally by `DifficultyManager`. Making HeavyEnemy slower would create **unfair perceptions** — the player might feel rewarded by the speed difference rather than their own skill. The intended challenge is *requiring more hits under the same time pressure*. This is more satisfying and tests timing skill directly.

### Why knockback was shortened

| Value | Phase 7 | Phase 8 |
|-------|---------|---------|
| `knockbackDistance` | 1.0 units | 0.45 units |
| `knockbackDuration` | 0.12 s | 0.08 s |

With `1.0` distance the HeavyEnemy could exit `attackRange` after the first hit, forcing the player to wait for it to walk back in before landing the second hit. This felt like punishing the player for hitting correctly. With `0.45` the enemy stays within range throughout the brief knockback and immediately resumes approach, keeping the rhythm tight.

**Tuning range:** `knockbackDistance 0.35–0.65`, `knockbackDuration 0.06–0.12` depending on `attackRange` setting.

---

## 4. Inspector Checklist

### EnemySpawner
- [ ] `enemyPrefabs` array is visible in the Inspector.
- [ ] Array size = `2`.
- [ ] `enemyPrefabs[0]` = **NormalEnemy** prefab.
- [ ] `enemyPrefabs[1]` = **HeavyEnemy** prefab.
- [ ] Single `enemyPrefab` fallback slot assigned (e.g. NormalEnemy) or intentionally left empty.
- [ ] `Player Transform` still assigned.
- [ ] `Difficulty Manager` still assigned.
- [ ] `Log Spawned Enemy Type` = `false` (unless debugging).

### NormalEnemy prefab
- [ ] `maxHealth = 1`
- [ ] `scoreValue = 100`
- [ ] `moveSpeed = 2.5`
- [ ] `knockbackDistance = 0.45` ← **manually update from 1.0**
- [ ] `knockbackDuration = 0.08` ← **manually update from 0.12**
- [ ] Sprite color = **red**
- [ ] Scale = `(0.7, 0.7, 1)`

### HeavyEnemy prefab
- [ ] `maxHealth = 2`
- [ ] `scoreValue = 200`
- [ ] `moveSpeed = 2.5` ← same as NormalEnemy
- [ ] `knockbackDistance = 0.45` ← **manually update from 1.0**
- [ ] `knockbackDuration = 0.08` ← **manually update from 0.12**
- [ ] Sprite color = **purple / dark blue / dark red** (visually distinct from NormalEnemy)
- [ ] Scale = `(0.85, 0.85, 1)`
- [ ] `logEnemyHits = false` (or `true` while debugging HeavyEnemy hits)

### Carry-over checks
- [ ] `GameUI` references still assigned (scoreText, comboText, panel, etc.).
- [ ] Phase 4 feedback slots still assigned (leftAttackIndicator, hitEffectPrefab, cameraShake).
- [ ] `GameManager → comboWindowDuration` still set (~1.0).
- [ ] `PlayerCombat → attackRange` still set (~2.0).

---

## 5. How to Test – Multiple Enemy Spawning

1. Press **Play**.
2. Observe the Hierarchy: enemies with `NormalEnemy` and `HeavyEnemy` names should both appear over time.
3. ✅ Both can spawn from left and right.
4. ✅ Both move at the same visual speed.
5. ✅ Difficulty scaling applies to both: as time passes, all enemies move faster.
6. Enable `logSpawnedEnemyType = true` on `EnemySpawner` to confirm variety in the Console:
   ```
   EnemySpawner: Spawned enemy prefab: NormalEnemy on the Left side.
   EnemySpawner: Spawned enemy prefab: HeavyEnemy on the Right side.
   ```

---

## 6. How to Test – NormalEnemy

1. Hit NormalEnemy once with the correct direction.
2. ✅ Console: `Hit registered! Combo: 1`
3. ✅ Console: `Enemy defeated! Score: 110, Combo: 1, Gain: 110`
4. ✅ Enemy is **destroyed** immediately. Score UI updates.

---

## 7. How to Test – HeavyEnemy (2-hit)

1. Wait for a HeavyEnemy to enter `attackRange`. Press correct direction (**hit 1**).
2. ✅ Console: `Hit registered! Combo: N`
3. ✅ Combo UI increases. Score UI does **not** change yet.
4. ✅ HeavyEnemy moves back ~0.45 units very briefly, then immediately resumes approach.
5. ✅ Enemy is still within or very near `attackRange`.
6. Press correct direction again (**hit 2**) while enemy is still close.
7. ✅ Console: `Hit registered! Combo: N+1`
8. ✅ Console: `Enemy defeated! Score: ..., Combo: N+1, Gain: 200 + (N+1)×10`
9. ✅ Enemy is **destroyed**. Score UI updates.

---

## 8. How to Test – Combo Rhythm with HeavyEnemy

1. Hit HeavyEnemy once → Combo: 1.
2. Wait for it to return to range (should be nearly instant with 0.08 s knockback).
3. Hit again within `comboWindowDuration` (1.0 s default).
4. ✅ Combo reaches 2. Second hit feels natural, not rushed or forced.
5. If the second hit feels too rushed or the enemy feels too close, increase `knockbackDistance` toward `0.60`.
6. If the enemy exits `attackRange` after knockback, decrease `knockbackDistance` toward `0.35` **or** increase `attackRange` slightly.

---

## 9. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| Enemy spawn | Enemies from left/right, mix of Normal and Heavy |
| Difficulty scaling | Speed and interval increase with elapsed time |
| Enemy movement | All enemies move at the same speed |
| NormalEnemy (1 hit) | Killed in one hit; score + combo awarded |
| HeavyEnemy (2 hits) | Survives first hit, knocked back, killed on second |
| Combo increases | On every valid hit (lethal or non-lethal) |
| Combo timeout | Resets after `comboWindowDuration` with no hit |
| Miss stun | Stun, tint, camera shake (if configured) |
| Score | Only increases on enemy defeat |
| Attack indicators | Flash on key press |
| Hit effect | Appears on both lethal and non-lethal hits |
| Camera shake on miss | Light shake |
| Camera shake on game over | Stronger shake |
| Game Over panel | Appears when any enemy touches player |
| Restart | Scene reloads; all state resets |

---

## 10. Known Limitations

- ❌ Enemy selection is simple 50/50 — no spawn weights yet.
- ❌ No lane blocking: if a HeavyEnemy is alive and a NormalEnemy spawns behind it, the NormalEnemy can clip through.
- ❌ No pattern enemies yet.
- ❌ No side-switch enemies yet.
- ❌ No combo shield / miss forgiveness yet.
- ❌ Knockback values `0.45 / 0.08` are starting points — tune per `attackRange` setting.
- ❌ Existing Phase 7 prefabs retain old knockback values (`1.0 / 0.12`) until manually updated.

---

## 11. Next Phase – Phase 9 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Lane blocking** | Track the closest enemy per side in `PlayerCombat`. Only allow hitting the frontmost one. |
| **Enemy queue awareness** | When HeavyEnemy is hit, enemies behind it should not suddenly accelerate past it. |
| **Spawn weights** | Add a `float[] spawnWeights` parallel array to `EnemySpawner`; use weighted random selection. |
| **Visual health state** | On non-lethal hit, tint HeavyEnemy to orange/yellow using `SpriteRenderer.color`. |
| **HP bar (optional)** | Small `Image` fill on the enemy prefab; updated in `TakeHit()`. |
