# Phase 7 – Enemy Health & Knockback Setup Guide

## 1. What Was Added in Phase 7

| Feature | Details |
|---------|---------|
| **Enemy health** | `maxHealth` field on each enemy. `currentHealth` starts at `maxHealth`. |
| **Multi-hit enemies** | `TakeHit()` reduces health by 1 per call. Only returns `true` (defeated) when health reaches 0. |
| **Knockback** | A non-lethal hit pushes the enemy back in its spawn direction for `knockbackDuration` seconds. |
| **Score only on defeat** | `RegisterEnemyDefeated()` is only called when health reaches 0. |
| **Combo on every valid hit** | `RegisterSuccessfulHit()` is called for every accurate hit regardless of whether the enemy died. |

---

## 2. Files Created / Changed

### `Enemy.cs` — **updated**
New serialized fields:
- `maxHealth` (default 1) — hits required to defeat.
- `knockbackDistance` (default 1.0) — units pushed back per non-lethal hit.
- `knockbackDuration` (default 0.12) — seconds the knockback movement takes.
- `logEnemyHits` (default false) — optional per-enemy Console logging.

New private fields: `currentHealth`, `isKnockedBack`.

New public properties: `CurrentHealth`, `MaxHealth`, `IsDefeated`.

New public method `TakeHit()`:
- Decrements `currentHealth` by 1.
- Returns `true` if defeated, `false` if still alive.
- Starts `KnockbackRoutine()` on a non-lethal hit.

New private `KnockbackRoutine()`:
- Sets `isKnockedBack = true`, lerps enemy backward over `knockbackDuration` seconds, then clears the flag.
- Normal movement in `Update()` is paused while `isKnockedBack` is true.
- Aborts safely if game ends during knockback.

`Initialize()`, `SetMoveSpeed()`, `ScoreValue`, `SpawnSide`, `OnTriggerEnter2D` — **all preserved unchanged**.

### `PlayerCombat.cs` — **updated** (hit branch only)
Old hit branch: `RegisterSuccessfulHit()` → `RegisterEnemyDefeated()` → Destroy.

New hit branch:
1. `RegisterSuccessfulHit()` — always (combo++).
2. `bool defeated = enemy.TakeHit()` — damage.
3. If `defeated` → `RegisterEnemyDefeated()`, `SpawnHitEffect()`, `Destroy`.
4. If alive → `SpawnHitEffect()` (hit flash), nothing else.

Miss branch, stun, indicators, camera shake — **all unchanged**.

### `EnemySpawner.cs` — **no changes**
### `DifficultyManager.cs` — **no changes**
### `GameManager.cs` — **no changes**
### `GameUI.cs` — **no changes**

---

## 3. Important Design Explanation

| | Phase 6 (old) | Phase 7 (new) |
|---|---|---|
| **When combo increases** | Every valid hit | Every valid hit (unchanged) |
| **When score increases** | Enemy defeat | Enemy defeat (unchanged) |
| **Hits to defeat enemy** | Always 1 | Configurable via `maxHealth` |
| **Non-lethal hit result** | N/A | Knockback, no score, combo still counts |

**Why enemy types should NOT differ by speed:**  
Speed scaling is handled globally by `DifficultyManager`. If a HeavyEnemy moved slower, it would feel like a cheat — the player has more time just because it has more HP. Instead, difficulty comes from requiring *more accurate hits within the same time pressure*, which rewards timing skill.

---

## 4. Prefab Setup – NormalEnemy & HeavyEnemy

Both prefabs use the same `Enemy.cs` script. Duplicate the existing Enemy prefab and change only the Inspector values:

### NormalEnemy prefab
| Field | Value |
|-------|-------|
| `maxHealth` | `1` |
| `scoreValue` | `100` |
| `moveSpeed` | `2.5` (overridden by DifficultyManager at runtime) |
| `knockbackDistance` | `1.0` (not triggered since 1 hit = defeat) |
| Sprite color | Red |
| Scale | `(0.7, 0.7, 1)` |

### HeavyEnemy prefab
| Field | Value |
|-------|-------|
| `maxHealth` | `2` |
| `scoreValue` | `200` |
| `moveSpeed` | `2.5` ← **keep equal to NormalEnemy** |
| `knockbackDistance` | `1.0` |
| `knockbackDuration` | `0.12` |
| Sprite color | Purple / dark blue |
| Scale | `(0.85, 0.85, 1)` |

> **Note**: EnemySpawner still supports only one prefab slot in this phase.
> Assign whichever prefab you want to test. Multi-prefab spawning comes in Phase 8.

---

## 5. Inspector Checklist

### Enemy prefab
- [ ] `Enemy.cs` is attached.
- [ ] `Max Health` field is visible and set correctly.
- [ ] `Score Value` field is visible and set correctly.
- [ ] `Knockback Distance` field is visible (default `1.0`).
- [ ] `Knockback Duration` field is visible (default `0.12`).
- [ ] `Log Enemy Hits` is unchecked unless debugging.

### NormalEnemy
- [ ] `maxHealth = 1`
- [ ] `scoreValue = 100`

### HeavyEnemy
- [ ] `maxHealth = 2`
- [ ] `scoreValue = 200`
- [ ] `moveSpeed = 2.5` (same as NormalEnemy — do NOT make it slower)

### PlayerCombat (on Player)
- [ ] `attackRange` still set (e.g. `2.0`).
- [ ] `stunDuration` still set (e.g. `0.4`).
- [ ] Feedback slots still assigned (indicators, hitEffectPrefab, cameraShake).

### EnemySpawner
- [ ] Enemy Prefab slot filled (NormalEnemy or HeavyEnemy for testing).
- [ ] Player Transform slot filled.
- [ ] DifficultyManager slot filled (if used).

### GameManager
- [ ] `comboWindowDuration` still set (e.g. `1.0`).

---

## 6. How to Test – NormalEnemy (maxHealth = 1)

1. Assign **NormalEnemy** to EnemySpawner. Press **Play**.
2. Wait for enemy to enter range. Press correct direction.
3. ✅ Console: `Hit registered! Combo: 1`
4. ✅ Console: `Enemy defeated! Score: 110, Combo: 1, Gain: 110`
5. ✅ Enemy is **destroyed**.
6. ✅ Hit effect appears at enemy position.
7. ✅ Combo UI shows 1, Score UI increases.

---

## 7. How to Test – HeavyEnemy (maxHealth = 2)

1. Assign **HeavyEnemy** to EnemySpawner. Press **Play**.
2. Wait for enemy to enter range. Press correct direction (**first hit**).
3. ✅ Console: `Hit registered! Combo: 1`
4. ✅ Combo UI shows 1.
5. ✅ Enemy is **NOT destroyed**.
6. ✅ Score UI does **NOT change**.
7. ✅ Enemy moves backward briefly (knockback), then resumes approach.
8. Press correct direction again (**second hit**).
9. ✅ Console: `Hit registered! Combo: 2`
10. ✅ Console: `Enemy defeated! Score: 320, Combo: 2, Gain: 220`
11. ✅ Enemy is **destroyed**.
12. ✅ Score UI increases by 220 (200 base + 2×10 combo bonus).

---

## 8. How to Test – Combo Timeout with HeavyEnemy

1. Hit HeavyEnemy once → Combo: 1.
2. Wait longer than `comboWindowDuration` (default 1.0 s) **without hitting again**.
3. ✅ Console: `Combo expired.`
4. ✅ Combo UI resets to 0.
5. Enemy is still alive. Hit it again.
6. ✅ Console: `Hit registered! Combo: 1` (new streak starts from 1).

---

## 9. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| Enemy spawn | Enemies appear from left/right at dynamic rate |
| Enemy movement | Enemies move toward player; stop on game over |
| Difficulty scaling | Speed and interval scale with elapsed time |
| NormalEnemy (1 hit) | Destroyed on first correct hit |
| Miss stuns player | Stun, color tint, camera shake |
| Score UI | Updates only on enemy defeat |
| Combo UI | Updates on every valid hit; resets on miss or timeout |
| Attack indicators | Flash on A/D / arrow press |
| Hit effect | Spawns on both lethal and non-lethal hits |
| Camera shake on miss | Light shake (if configured) |
| Camera shake on game over | Stronger shake (if configured) |
| Game Over panel | Appears when any enemy touches player |
| Restart button | Reloads scene; resets everything |

---

## 10. Known Limitations

- ❌ EnemySpawner supports only one prefab — NormalEnemy and HeavyEnemy cannot mix yet.
- ❌ No lane blocking yet.
- ❌ No pattern enemies (e.g. always spawning from one side) yet.
- ❌ No side-switch enemies yet.
- ❌ No combo shield / miss forgiveness yet.
- ❌ Knockback is simple lerp — no animation curve or bounce yet.
- ❌ No visual health indicator (e.g. HP bar or color progression) yet.

---

## 11. Next Phase – Phase 8 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Multiple prefab spawning** | Add `GameObject[] enemyPrefabs` array to `EnemySpawner`; pick randomly each spawn. |
| **Spawn weights** | Add a parallel `float[] spawnWeights` array; use weighted random selection. |
| **Visual health feedback** | On non-lethal hit, tint enemy to orange/yellow using `SpriteRenderer.color`. |
| **HP bar** | Simple UI `Image` fill on the enemy prefab, updated in `TakeHit()`. |
| **Consistent speed** | All prefabs get the same speed from DifficultyManager — enforce this in documentation. |
