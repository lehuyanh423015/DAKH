# Phase 14 – Weighted Random Enemy Spawning Setup Guide

## 1. What Was Added in Phase 14

| Feature | Details |
|---------|---------|
| **Weighted Random Spawning** | The `EnemySpawner` now supports an array of spawn weights corresponding to the `enemyPrefabs` array. |
| **Pacing Control** | Complex pattern enemies (like SwitchEnemy and PatternEnemy3Hit) can now be configured to spawn far less frequently than standard enemies, keeping the endless mode readable and fair. |
| **Safe Fallbacks** | If spawn weights are misconfigured (e.g. missing, length mismatch, or all zero), the spawner safely falls back to equal random selection and logs a single warning. |

---

## 2. Files Created / Changed

### `EnemySpawner.cs` — **updated**
- Added `enemySpawnWeights` float array.
- Added `logSpawnWeights` boolean for debug logging.
- Refactored `SpawnEnemy` into `PickPrefab()`, `PickWeightedPrefab()`, and `PickEqualRandomPrefab()`.
- Added validation to prevent errors from mismatched array lengths or null elements.

---

## 3. Important Design Explanation

### Why Pacing Matters
In an endless combat game, difficulty should come from pattern complexity, pressure, and pacing—not just speed. If 4 enemy types spawn with equal probability, the screen quickly becomes a chaotic mix of overlapping patterns that feel unfair. 
By heavily weighting the `NormalEnemy` (e.g. 60%) and keeping `PatternEnemy3Hit` rare (e.g. 5%), the game establishes a predictable baseline rhythm that is occasionally interrupted by high-threat patterns. This creates a much more engaging difficulty curve.

---

## 4. Inspector Checklist

### EnemySpawner
Select the **EnemySpawner** GameObject in the scene and set the following:

- [ ] `enemyPrefabs` Size = **4**
  - `Element 0` = NormalEnemy
  - `Element 1` = HeavyEnemy
  - `Element 2` = SwitchEnemy
  - `Element 3` = PatternEnemy3Hit
- [ ] `enemySpawnWeights` Size = **4**
  - `Element 0` = **60**  *(NormalEnemy)*
  - `Element 1` = **25**  *(HeavyEnemy)*
  - `Element 2` = **10**  *(SwitchEnemy)*
  - `Element 3` = **5**   *(PatternEnemy3Hit)*
- [ ] `DifficultyManager` still assigned.
- [ ] `Player Transform` still assigned.
- [ ] `logSpawnWeights` = **False** (unless debugging).

---

## 5. How to Test Weighted Spawning

1. Select `EnemySpawner` and set `logSpawnWeights = true`.
2. Press **Play**.
3. Watch the Console output. You should see logs like:
   `EnemySpawner: Selected NormalEnemy using weighted spawn.`
4. Confirm that `NormalEnemy` appears the vast majority of the time.
5. Confirm `PatternEnemy3Hit` is quite rare.
6. **Remember to disable** `logSpawnWeights` when finished testing so the console doesn't get spammed.

---

## 6. How to Test Fallback Behavior

1. Temporarily clear the `enemySpawnWeights` array (set Size to 0) OR change the Size to 3.
2. Press **Play**.
3. The Console should log a **single warning**:
   `EnemySpawner: Spawn weights invalid... Falling back to equal random selection.`
4. Enemies should spawn evenly across all types.
5. Restore the array size to 4 and re-enter the correct weights after testing.

---

## 7. Regression Test

| Feature | Expected Result |
|---------|----------------|
| **NormalEnemy** | Dies in 1 hit |
| **HeavyEnemy** | Knockback on hit 1, dies on hit 2 |
| **SwitchEnemy** | Switches side on hit 1, dies on hit 2 |
| **PatternEnemy3Hit**| Alternates sides on hits 1 & 2, dies on hit 3 |
| **Lane Blocking** | Enemies queue up correctly on the same side |
| **Player Shift** | Player subtly shifts after attacks |
| **Camera Follow** | Smoothly tracks player X position |
| **Combo & Scoring** | Combo per hit, score per defeat. Timeout/miss resets combo |
| **Difficulty Scaling** | Global speed and interval scale over time |

---

## 8. Known Limitations

- **Static Weights:** The weights currently remain static throughout the entire run. They do not dynamically adjust as the player's score or combo goes up.
- **True Random Clustering:** Because the selection is standard weighted random, it's statistically possible (though unlikely) to get 4 `PatternEnemy3Hit`s in a row. A "deck" or "bag" system would be needed to absolutely prevent clusters.
- **No Boss/Wave System:** Endless mode relies purely on interval scaling rather than distinct waves or bosses.

---

## 9. Next Phase Suggestion

**Phase 15:**
- Implement a **Combo Shield / Miss Forgiveness** mechanic to ease the frustration of long combo drops.
- OR implement **Dynamic Spawn Weights** that shift the probabilities over time (e.g., more complex enemies appear later).
- OR add **Sound Effects** if the gameplay and pacing finally feel stable.
