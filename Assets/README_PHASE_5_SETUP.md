# Phase 5 – Difficulty Scaling Setup Guide

## 1. What Was Added in Phase 5

| Feature | Details |
|---------|---------|
| **Time-based difficulty** | A `DifficultyManager` tracks elapsed gameplay time and grows a multiplier linearly. |
| **Enemy speed scaling** | Each newly spawned enemy moves at `baseEnemySpeed × multiplier`, capped at `maxEnemySpeed`. |
| **Spawn interval scaling** | The gap between spawns shrinks to `baseSpawnInterval ÷ multiplier`, floored at `minSpawnInterval`. |
| **Min/max caps** | Speed never exceeds `maxEnemySpeed`; interval never goes below `minSpawnInterval`. |
| **Graceful fallback** | If `DifficultyManager` is not assigned to `EnemySpawner`, everything works as Phase 1–4 (fixed speed, fixed interval). |
| **Debug logging** | Every 10 s, optionally logs multiplier, speed, and interval to the Console. |

---

## 2. Files Created / Changed

### `DifficultyManager.cs` — **created** (`Assets/Scripts/Core/DifficultyManager.cs`)
New script. Tracks `elapsedTime`, exposes `DifficultyMultiplier`, `CurrentEnemySpeed`, `CurrentSpawnInterval`. Stops updating on game over. Optional debug logging.

### `EnemySpawner.cs` — **updated**
- New `[SerializeField] private DifficultyManager difficultyManager;` slot.
- In `Update()`: uses `difficultyManager.CurrentSpawnInterval` when assigned, falls back to fixed `spawnInterval`.
- In `SpawnEnemy()`: calls `enemyScript.SetMoveSpeed(difficultyManager.CurrentEnemySpeed)` when assigned.

### `Enemy.cs` — **updated**
- New `public void SetMoveSpeed(float speed)` method.
- Speed can now be overridden at spawn time without breaking the prefab default.

### `GameManager.cs` — **no changes**
### `PlayerCombat.cs` — **no changes**
### `GameUI.cs` — **no changes**
### `CameraShake.cs` — **no changes**
### `TemporaryEffect.cs` — **no changes**

---

## 3. Scene Setup Checklist

### New object
- [ ] Empty GameObject named **`DifficultyManager`** created in MainScene.
- [ ] `DifficultyManager.cs` is attached to it.

### EnemySpawner wiring
- [ ] Select `EnemySpawner` in the Hierarchy.
- [ ] Drag the `DifficultyManager` GameObject into the **"Difficulty Manager"** slot on `EnemySpawner`.

### Existing wiring (unchanged)
- [ ] `EnemySpawner` still has **Enemy Prefab** assigned.
- [ ] `EnemySpawner` still has **Player Transform** assigned.
- [ ] Enemy prefab still has `Enemy.cs` attached.
- [ ] `GameManager` object still has `GameManager.cs` attached.
- [ ] All `GameUI` references still assigned (scoreText, comboText, gameOverPanel, finalScoreText, restartButton).
- [ ] All Phase 4 feedback slots still assigned (indicators, hitEffectPrefab, cameraShake) if used.

---

## 4. Recommended Inspector Values

### DifficultyManager
| Field | Recommended Value | Notes |
|-------|-------------------|-------|
| `baseEnemySpeed` | `2.5` | Matches Enemy prefab default |
| `maxEnemySpeed` | `6.0` | Hard cap — never exceeded |
| `baseSpawnInterval` | `1.5` | Matches EnemySpawner fallback |
| `minSpawnInterval` | `0.45` | Minimum gap — prevents impossibly fast spawns |
| `difficultyIncreaseRate` | `0.05` | 5% multiplier growth per second |
| `logDifficulty` | `true` | Disable after testing |
| `logInterval` | `10` | Log every 10 seconds |

### Difficulty timeline (reference)
| Elapsed time | Multiplier | Enemy speed | Spawn interval |
|-------------|-----------|-------------|---------------|
| 0 s | 1.00× | 2.50 | 1.50 s |
| 10 s | 1.50× | 3.75 | 1.00 s |
| 20 s | 2.00× | 5.00 | 0.75 s |
| 30 s | 2.50× | 6.00 (cap) | 0.60 s |
| 50 s | 3.50× | 6.00 (cap) | 0.45 s (cap) |

---

## 5. How to Test Difficulty Scaling

1. Press **Play**.
2. Watch the first few enemies — they should move at approximately `2.5` units/second.
3. Wait **20–30 seconds** while surviving.
4. Notice enemies noticeably faster and spawning more frequently.
5. Check the Console every 10 s for a log like:
   ```
   Difficulty: multiplier=2.00, enemySpeed=5.00, spawnInterval=0.75
   ```
6. Let the game run to ~50 s — confirm speed stays at `6.0` (cap) and interval stays at `0.45 s` (cap).
7. Trigger **Game Over** — confirm the difficulty log stops appearing (scaling paused).
8. Click **Restart** — scaling resets to 1× after scene reload.

---

## 6. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| Enemy spawn | Enemies still appear from left/right (now at dynamic rate) |
| Enemy movement | Enemies move toward player (now at dynamic speed), stop on game over |
| Hit destroys enemy | Correct-side attack within range kills the closest enemy |
| Score UI | Updates after every kill |
| Combo UI | Increments per kill, resets on miss |
| Miss stun | Player stunned, color tints, combo resets |
| Game Over panel | Appears when enemy touches player |
| Final score display | Shows correct accumulated score |
| Restart button | Reloads scene, resets everything including difficulty |
| Attack indicators | Left/right sprites flash on key press |
| Hit effect | Prefab spawns at enemy position on kill |
| Camera shake | Light shake on miss, stronger on game over (if configured) |

---

## 7. Known Limitations

- ❌ Difficulty is purely time-based — no skill-adaptive scaling.
- ❌ No wave system yet (enemies spawn continuously, not in waves).
- ❌ No enemy variety yet — only one enemy type with scaled speed.
- ❌ No balancing polish — tune `difficultyIncreaseRate`, `maxEnemySpeed`, and `minSpawnInterval` to taste.
- ❌ Existing on-screen enemies keep their spawned speed — mid-game speed changes only apply to newly spawned enemies.
- ❌ No sound effects yet.

---

## 8. Next Phase – Phase 6 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Enemy variety** | Add a second enemy prefab with higher `scoreValue` and `moveSpeed`. Randomly choose between them in `EnemySpawner`. |
| **Wave system** | Spawn enemies in numbered waves with a short break between each. |
| **Sound effects** | `AudioSource.PlayOneShot()` in `PlayerCombat` for hit/miss, and in `Enemy` on game over. |
| **UI polish** | Animate score/combo text scale on change; slide the Game Over panel in. |
| **Difficulty display** | Show elapsed time or difficulty level on the HUD. |
| **Better pacing** | Increase `difficultyIncreaseRate` nonlinearly (e.g. use a curve). |
