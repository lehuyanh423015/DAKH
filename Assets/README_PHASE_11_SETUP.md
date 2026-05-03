# Phase 11 – Smooth Camera Follow Setup Guide

## 1. What Was Added in Phase 11

| Feature | Details |
|---------|---------|
| **Smooth camera follow** | Camera tracks the player's X position with a lerp-based lag, not a hard lock. |
| **followFactor** | Camera moves only 60% of the player's X offset — it never fully centres on the player, preserving battlefield readability. |
| **Dead zone** | Camera ignores deviations smaller than `deadZone` (default 0.4) to prevent jitter from tiny attack shifts. |
| **X bounds** | Optional `[minX, maxX]` clamp keeps the camera from drifting past a set limit. |
| **CameraShake compatibility** | `CameraShake` now snapshots world position at shake-start so it oscillates around the followed position, not the original scene origin. |

---

## 2. Files Created / Changed

### `Assets/Scripts/Camera/CameraFollow.cs` — **created**
New script. Attach to Main Camera. Responsibilities:
- `LateUpdate` loop: reads `target.position.x`, computes `desiredX = initialCameraX + playerX × followFactor`, checks dead zone, lerps camera X.
- Preserves `initialPosition.y` (camera Y never moves).
- Applies `[minX, maxX]` clamp when `useBounds = true`.
- `logCameraFollow` flag for Console debugging.

### `Assets/Scripts/Feedback/CameraShake.cs` — **updated** (minimal)
Changed `ShakeRoutine` to snapshot **world** `transform.position` at shake-start (`basePosition = transform.position`) instead of `localPosition`. The shake now oscillates around the already-followed position and restores to it. `CameraFollow.LateUpdate` then corrects any remaining deviation on the next frame with no visible snap.

Old behaviour: captured `transform.localPosition` once at scene start. If the camera followed to X = 0.6, the restore snapped back to X = 0.
New behaviour: captures current world position at each shake invocation. Restore targets the followed position.

### All other scripts — **no changes**
`PlayerCombat.cs`, `PlayerMovement.cs`, `Enemy.cs`, `EnemySpawner.cs`, `GameManager.cs`, `GameUI.cs`, `DifficultyManager.cs`, `TemporaryEffect.cs`

---

## 3. Important Design Explanation

### Camera follows lightly, not fully
`followFactor = 0.6` means if the player is at X = 2.0, the camera targets X = `initialCameraX + 2.0 × 0.6 = initialCameraX + 1.2`. The player is never perfectly centred — the camera gives them breathing room to see what is coming from the side they moved toward.

### Dead zone prevents jitter
Player shifts are only 0.25 units per attack. With `deadZone = 0.4`, a single shift does **not** move the camera at all. Only after 2+ shifts in the same direction does the camera start to follow. This keeps the camera stable during fast combat.

### Y axis is fixed
`transform.position = new Vector3(newX, initialPosition.y, current.z)` — camera Y is always restored to the initial scene value. The camera never bobs up or down.

### CameraFollow and CameraShake coexist
Both scripts run on the same camera Transform. Execution order:
1. **CameraShake** runs inside a coroutine (Update timing) — applies random offset.
2. **CameraFollow** runs in `LateUpdate` — moves camera toward desired X.

Because LateUpdate runs after Update, CameraFollow will override the shake offset on the same frame — meaning shake may be slightly dampened horizontally. The vertical shake (Y offset) is **not** overridden because `CameraFollow` only writes to X. If you want full shake freedom, set `followFactor = 0` during shake (not implemented, not needed for this prototype).

---

## 4. Inspector Checklist

### Main Camera
- [ ] `CameraFollow.cs` is attached.
- [ ] `CameraShake.cs` is still attached (if shake is used).
- [ ] `CameraFollow → Target` = **Player** transform.
- [ ] `followStrength` = `3.0`
- [ ] `followFactor` = `0.6`
- [ ] `deadZone` = `0.4`
- [ ] `useBounds` = ✓
- [ ] `minX` = `-1.5`
- [ ] `maxX` = `1.5`
- [ ] `logCameraFollow` = ✗

### Player GameObject (unchanged from Phase 10)
- [ ] `PlayerMovement.cs` still attached.
- [ ] `PlayerMovement → minX` = `-2.5`, `maxX` = `2.5`
- [ ] `PlayerCombat` references still intact.

### Everything else
- [ ] `EnemySpawner → enemyPrefabs` still assigned.
- [ ] `GameUI` references still assigned.
- [ ] Both enemy prefabs have `enableLaneBlocking = true`.
- [ ] `GameManager → comboWindowDuration` still set.

---

## 5. How to Test – Camera Follow

1. Press **Play**.
2. Press **A** several times (3–4 rapid presses) to drift the player left.
3. ✅ Camera follows slowly to the left — not an instant snap.
4. ✅ Camera does **not** reach the player's exact X (followFactor = 0.6 offsets less).
5. Press **D** several times to drift right.
6. ✅ Camera follows back toward centre/right.
7. Press A/D at bounds extremes.
8. ✅ Camera stops at `minX = -1.5` / `maxX = 1.5` even if player is at ±2.5.

**Fine-tuning:**
| Too static? | Increase `followFactor` (toward 0.8) or decrease `deadZone` (toward 0.2). |
| Too aggressive? | Decrease `followFactor` (toward 0.4) or increase `deadZone` (toward 0.6). |
| Lag too high? | Increase `followStrength` (toward 5.0). |
| Lag too low? | Decrease `followStrength` (toward 1.5). |

---

## 6. How to Test – Dead Zone

1. Press A once (single shift = 0.25 units).
2. ✅ Camera does **not** move (0.25 < deadZone 0.4).
3. Press A twice more rapidly.
4. ✅ Camera starts following after accumulated player movement exceeds the dead zone.
5. Increase `deadZone` if camera feels too eager; decrease if it feels too static.

---

## 7. How to Test – CameraShake Compatibility

1. Miss an attack intentionally.
2. ✅ Camera shakes with the configured miss shake.
3. ✅ After shake, camera returns to the **followed position** (not the original scene origin).
4. Shift the player left (several A presses) then miss.
5. ✅ Shake happens at the new followed position, not at X = 0.
6. Trigger Game Over (let enemy reach player).
7. ✅ Stronger game-over shake still fires.
8. ✅ Camera lands back at followed position after shake ends.

---

## 8. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| Player shift | A/D shifts player within ±2.5 bounds |
| Camera follow | Camera lags behind player X; stays within ±1.5 |
| NormalEnemy (1 hit) | Destroyed, combo+1, score+ |
| HeavyEnemy (2 hits) | Survives hit 1 (knockback), destroyed on hit 2 |
| Lane blocking | Queue preserved; auto-resume on front death |
| Enemy tracking | Enemies follow player's current (shifted) position |
| Combo on every hit | Lethal and non-lethal |
| Combo timeout | Resets after `comboWindowDuration` |
| Miss stun | Stun, tint, camera shake |
| Score | Only on defeat |
| Attack indicators | Flash on key press |
| Hit effect | Both lethal and non-lethal |
| Difficulty scaling | Speed + interval increase over time |
| Game Over panel | Appears on enemy collision |
| Restart | Scene reloads; all state resets |
| Canvas UI | Score/Combo/GameOver panel unaffected by camera movement (Screen Space Overlay) |

---

## 9. Known Limitations

- ❌ Spawn positions (`leftSpawnPosition`, `rightSpawnPosition`) remain fixed — enemies still enter from fixed world edges, not relative to camera.
- ❌ Camera horizontal shake is slightly dampened by `CameraFollow.LateUpdate` overriding the X offset. Vertical shake is unaffected.
- ❌ Camera follow is simple transform lerp — no Cinemachine, no damping curves.
- ❌ `followFactor` and `deadZone` require manual tuning per `attackRange` / `PlayerMovement.shiftDistance` settings.
- ❌ No zoom behavior.
- ❌ No advanced framing or look-ahead.

---

## 10. Next Phase – Phase 12 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Adaptive spawn framing** | Offset `leftSpawnPosition` and `rightSpawnPosition` relative to player X each spawn so enemies always enter from screen edges. |
| **Pattern enemy (side-switch)** | After a HeavyEnemy is hit for the first time, it reverses direction (switches lane from Left to Right). Requires changing `Side` and negating movement direction in `Enemy.Update`. |
| **Visual HP indicator** | On non-lethal hit, tint HeavyEnemy to orange/yellow via `SpriteRenderer.color`. Reset on death/spawn. |
| **Spawn weight tuning** | Add `float[] spawnWeights` parallel to `enemyPrefabs[]` in `EnemySpawner` for weighted random selection. |
