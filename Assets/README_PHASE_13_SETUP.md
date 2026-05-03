# Phase 13 – PatternEnemy3Hit (3-Hit Alternating Pattern) Setup Guide

## 1. What Was Added in Phase 13

| Feature | Details |
|---------|---------|
| **PatternEnemy3Hit** | A new 3-hit enemy that alternates sides after each non-lethal hit. |
| **AlternatingThreeHit behavior** | New enum value in `EnemyBehaviorType`. Functions similarly to `SwitchSideOnHit` but accommodates more health. |
| **Safe Side-Switch Reuse** | Reuses the mirrored, clamped, queue-aware side-switch logic introduced in Phase 12 Hotfix v2. |
| **Visual Feedback** | Added a simple color tinting effect: the enemy's sprite darkens slightly on each non-lethal hit to provide visual progression without complex UI. |

---

## 2. Files Created / Changed

### `Enemy.cs` — **updated**
- Added `AlternatingThreeHit` to the `EnemyBehaviorType` enum.
- Updated `TakeHit()` to trigger a side-switch for `AlternatingThreeHit`.
- Added visual feedback in `TakeHit()`: slightly multiplies the `SpriteRenderer` color by `0.75` on each non-lethal hit to indicate damage progression for pattern enemies.

### `PlayerCombat.cs` & `EnemySpawner.cs` — **no changes**
The existing side detection logic in `PlayerCombat` and array-based spawning in `EnemySpawner` continue to work without modification.

---

## 3. Important Design Explanation

### Extended Pattern Complexity
`PatternEnemy3Hit` is harder than `SwitchEnemy` not because it moves faster, but because it demands a longer sequence of correct inputs from the player while under pressure.
The rhythm is:
1. Hit current side
2. Hit opposite side
3. Hit original side

### Scoring and Combo
Like all enemies, the combo increases on **every valid hit**, rewarding the player for maintaining the rhythm. However, the score is only awarded upon **defeat** (the 3rd hit), ensuring the player finishes the pattern to get the points.

---

## 4. Inspector Checklist

### PatternEnemy3Hit Prefab
**Steps to create:**
1. Duplicate the **SwitchEnemy** prefab.
2. Rename to **PatternEnemy3Hit**.
3. Set the following in the Inspector:

- [ ] `behaviorType` = `AlternatingThreeHit`
- [ ] `maxHealth` = `3`
- [ ] `scoreValue` = `350`
- [ ] `moveSpeed` = `2.5` (Same as all other enemies)
- [ ] `sideSwitchGap` = `0.2`
- [ ] `rehitMargin` = `0.15`
- [ ] `postSwitchNoGameOverDuration` = `0.06`
- [ ] `minSpacingOnTargetSide` = `0.7`
- [ ] `targetFollowupMaxDistance` = `2.0` (Match PlayerCombat's attackRange)
- [ ] `sideSwitchDuration` = `0.08`
- [ ] `enableLaneBlocking` = ✓
- [ ] `minSpacingFromFrontEnemy` = `0.8`
- [ ] **Sprite Color** = Distinct color (e.g., bright cyan, magenta, or gold)
- [ ] **Transform Scale** = `(0.9, 0.9, 1)`

### EnemySpawner
- [ ] `enemyPrefabs` array size is 4.
- [ ] `enemyPrefabs[0]` = NormalEnemy
- [ ] `enemyPrefabs[1]` = HeavyEnemy
- [ ] `enemyPrefabs[2]` = SwitchEnemy
- [ ] `enemyPrefabs[3]` = PatternEnemy3Hit

---

## 5. How to Test

### Test 1 – PatternEnemy3Hit from Left
1. Temporarily isolate `PatternEnemy3Hit` in the spawner or wait for one.
2. Let it approach from the **left**.
3. Attack **left**.
   - ✅ Combo increases, enemy darkens slightly, crosses to the **right**.
4. Attack **right**.
   - ✅ Combo increases, enemy darkens further, crosses back to the **left**.
5. Attack **left**.
   - ✅ Combo increases, enemy is defeated, score increases.

### Test 2 – PatternEnemy3Hit from Right
1. Let it approach from the **right**.
2. Attack **right** → crosses Left.
3. Attack **left** → crosses Right.
4. Attack **right** → defeated.

### Test 3 – Wrong Side Behavior
1. Hit the enemy once (it switches sides).
2. Attack the old side.
   - ✅ Miss is registered, stun occurs, combo resets.
3. Attack the new side.
   - ✅ Hit connects, enemy switches back.

### Test 4 – Rhythm and Fairness
- ✅ After each switch, the enemy lands within follow-up attack range.
- ✅ No immediate Game Over occurs during or immediately after the side-switch.
- ✅ The enemy respects queue spacing and does not overlap with existing enemies on the destination side.

---

## 6. Regression Test

| Feature | Expected Result |
|---------|----------------|
| **NormalEnemy** | Dies in 1 hit |
| **HeavyEnemy** | Knockback on hit 1, dies on hit 2 |
| **SwitchEnemy** | Switches side on hit 1, dies on hit 2 |
| **Lane Blocking** | Enemies queue up correctly on the same side |
| **Player Shift** | Player subtly shifts after attacks |
| **Camera Follow** | Smoothly tracks player X position |
| **Combo & Scoring** | Combo per hit, score per defeat. Timeout/miss resets combo |
| **Difficulty Scaling** | Global speed and interval scale over time |

---

## 7. Known Limitations

- **Visual Feedback:** No explicit pattern UI indicators or HP bars. The color darkening is a minimal prototype-friendly cue.
- **Spawn Chaos:** With 4 enemy types spawning purely randomly, the screen can get chaotic. Weighted spawning is recommended for better pacing.
- **Punishing Mistakes:** A single miss still breaks the combo and stuns, which can be harsh in long patterns.

---

## 8. Next Phase Suggestion

**Phase 14:**
- Implement **weighted random spawning** to control the frequency of complex pattern enemies vs. normal enemies.
- Alternatively, introduce a **Combo Shield** (miss forgiveness) to make longer patterns less frustrating.
