# Phase 6 – Combo-by-Hit Refactor Setup Guide

## 1. What Was Added in Phase 6

| Feature | Details |
|---------|---------|
| **Combo-by-hit** | Combo now increments on every accurate hit, not only on enemy kills. |
| **Enemy defeat = score only** | Defeating an enemy awards score but does not itself increment the combo. |
| **Combo timeout** | If no accurate hit lands within `comboWindowDuration` seconds, combo resets to 0 automatically. |
| **Miss = instant reset** | Pressing an attack key with no valid target resets combo immediately (unchanged from Phase 2). |
| **Backward-compatible wrapper** | `RegisterHit(baseScore)` still exists and calls both new methods, so Phase 1–5 single-hit enemies work without changes. |

---

## 2. Files Created / Changed

### `GameManager.cs` — **updated**
Key changes:
- New `[SerializeField] float comboWindowDuration = 1.0` (Inspector-tunable).
- New private fields: `lastSuccessfulHitTime`, `hasActiveCombo`.
- **`RegisterSuccessfulHit()`** — new method; increments combo, refreshes timer, fires `OnScoreComboChanged`.
- **`RegisterEnemyDefeated(int baseScore)`** — new method; adds score using current combo, fires `OnScoreComboChanged`. Does NOT touch combo.
- **`RegisterHit(int baseScore)`** — kept as a wrapper calling both of the above.
- `Update()` — checks combo timeout each frame (only logs once when it actually expires).
- `RegisterMiss()` — now also sets `hasActiveCombo = false`.
- `ResetCombo()` — also sets `hasActiveCombo = false`.

### `PlayerCombat.cs` — **updated** (2-line change in `TryAttack`)
- Old hit branch: `GameManager.Instance.RegisterHit(enemy.ScoreValue)`
- New hit branch: `RegisterSuccessfulHit()` → `RegisterEnemyDefeated(enemy.ScoreValue)`
- All feedback (indicator, hit effect, camera shake, stun) is unchanged.

### `GameUI.cs` — **no changes**
Already listens to `OnScoreComboChanged`; UI updates on both combo increment and expiry.

### All other scripts — **no changes**
`Enemy.cs`, `EnemySpawner.cs`, `DifficultyManager.cs`, `CameraShake.cs`, `TemporaryEffect.cs` are unaffected.

---

## 3. Important Design Change

| | Phase 1–5 (old) | Phase 6 (new) |
|---|---|---|
| **Combo trigger** | Enemy kill | Accurate hit on any enemy |
| **Score trigger** | Same as combo (together) | Enemy defeat only |
| **Combo timeout** | Never (lasted forever until miss) | Resets after `comboWindowDuration` seconds |
| **Multi-hit ready** | ✗ (combo counted kills only) | ✓ (combo counts hits independently) |

**Why this matters for Phase 7:**  
A future HeavyEnemy might require 2 hits to kill. Under the old system, no kill = no combo. Under the new system, each hit extends the combo window, so skilled players are rewarded even before the enemy dies.

---

## 4. Inspector Checklist

- [ ] `GameManager` has `GameManager.cs` attached.
- [ ] `GameManager` component shows **`Combo Window Duration`** in the Inspector.
- [ ] `Combo Window Duration` is set to **1.0** (or tune to taste).
- [ ] `Player` still has `PlayerCombat.cs` attached.
- [ ] `PlayerCombat` `attackRange` still set (e.g. `2.0`).
- [ ] `PlayerCombat` `stunDuration` still set (e.g. `0.4`).
- [ ] All `GameUI` references still assigned (scoreText, comboText, gameOverPanel, etc.).
- [ ] `EnemySpawner` still has Enemy Prefab and Player Transform assigned.
- [ ] `DifficultyManager` still assigned to `EnemySpawner` if used.
- [ ] Phase 4 feedback slots (leftAttackIndicator, hitEffectPrefab, cameraShake) still assigned if used.

---

## 5. How to Test – Combo on Hit

1. Press **Play**.
2. Wait for an enemy to enter `attackRange`.
3. Press the correct direction key to kill it.
4. ✅ Console: `Hit registered! Combo: 1`
5. ✅ Console: `Enemy defeated! Score: 110, Combo: 1, Gain: 110`
6. ✅ Combo UI shows **1**.
7. Kill another enemy **within 1 second**.
8. ✅ Console: `Hit registered! Combo: 2`
9. ✅ Combo UI shows **2**.

---

## 6. How to Test – Combo Timeout

1. Build a combo (e.g. kill 2 enemies quickly → Combo: 2).
2. Stop attacking and **wait longer than `comboWindowDuration`** (default 1.0 s).
3. ✅ Console: `Combo expired.`
4. ✅ Combo UI resets to **0**.
5. ✅ Score is unchanged (timeout does not deduct score).

---

## 7. How to Test – Miss Reset

1. Build a combo of 3+.
2. Press an attack key with **no enemy in range** on that side.
3. ✅ Console: `Miss! Combo reset.`
4. ✅ Combo UI instantly resets to **0**.
5. ✅ Player stun color activates for `stunDuration` seconds.

---

## 8. How to Test – Score Formula

| Situation | Expected gain |
|-----------|--------------|
| Combo 1, baseScore 100 | `100 + 1 × 10 = 110` |
| Combo 2, baseScore 100 | `100 + 2 × 10 = 120` |
| Combo 3, baseScore 100 | `100 + 3 × 10 = 130` |

Confirm via the `Enemy defeated!` Console log:
```
Enemy defeated! Score: 110, Combo: 1, Gain: 110
Enemy defeated! Score: 230, Combo: 2, Gain: 120
Enemy defeated! Score: 360, Combo: 3, Gain: 130
```

---

## 9. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| Enemy spawn | Enemies appear from left/right at dynamic interval |
| Enemy movement | Enemies move toward player, stop on game over |
| Difficulty scaling | Speed and interval scale with elapsed time |
| Correct hit destroys enemy | One-hit kill still works |
| Miss stuns player | Stun, color tint, camera shake all still active |
| Attack indicators | Left/right sprites flash on key press |
| Hit effect prefab | Spawns at enemy position on kill |
| Camera shake on miss | Light shake (if configured) |
| Camera shake on game over | Stronger shake (if configured) |
| Score UI updates | Refreshes on every `RegisterEnemyDefeated` call |
| Combo UI updates | Refreshes immediately on `RegisterSuccessfulHit` and on timeout |
| Game Over panel | Appears when enemy touches player |
| Final score displayed | Correct accumulated score shown |
| Restart button | Reloads scene, resets everything |

---

## 10. Known Limitations

- ❌ Enemies are still one-hit kills in this phase — multi-hit is not yet implemented.
- ❌ No enemy knockback yet.
- ❌ No combo shield / miss-forgiveness window yet.
- ❌ `comboWindowDuration = 1.0` is a starting point — tune based on feel.
- ❌ No visual combo timer (e.g. shrinking arc around player) yet.
- ❌ No sound effects yet.

---

## 11. Next Phase – Phase 7 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Enemy health** | Add `private int currentHealth` and `TakeDamage()` to `Enemy.cs`. |
| **HeavyEnemy (2 hits)** | Set `scoreValue` higher and `currentHealth = 2` on a new prefab variant. |
| **Hit → combo without kill** | `PlayerCombat` calls `RegisterSuccessfulHit()` on any hit; only calls `RegisterEnemyDefeated()` when enemy health reaches 0. |
| **Knockback** | On non-lethal hit, push enemy backward along X by a small impulse for visual clarity. |
| **Consistent movement speed** | Keep `moveSpeed` identical across enemy types; differentiate enemies by health and score, not speed. |
