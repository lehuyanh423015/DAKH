# Phase 12 Hotfix v2 – Queue-Aware Mirrored Side-Switch Placement

## 1. What Was Wrong With Fixed Placement

| Problem | Root cause |
|---------|-----------|
| **Landing too far away** | Previous hotfix used `sideSwitchGap + halfWidths` as a fixed distance regardless of how close the enemy actually was. If the enemy was 3 units away, it still landed at ~2 units — destroying the second-hit rhythm. |
| **Ignores existing enemies on destination side** | Fixed landing could drop SwitchEnemy inside an existing queue, causing visual overlap or sending it past attackable range. |
| **No post-landing grace period** | The `isSwitchingSide` flag turned off exactly at landing. If the collider still briefly overlapped the player for one physics frame, Game Over fired. |
| **Distance measured from enemy X** | Old formula: `playerX ± safeOffset`. While player-relative, it did not account for how far the enemy was approaching. A far-away enemy would land at the same distance as a close one. |

---

## 2. New Formula

### Safe minimum distance (collision safety)
```
safeMinDistance = sideSwitchGap + playerHalfWidth + enemyHalfWidth
```
- `playerHalfWidth` = `playerCollider.bounds.extents.x` (fallback → sprite → 0.35)
- `enemyHalfWidth`  = `enemyCollider.bounds.extents.x`  (fallback → sprite → 0.35)
- `sideSwitchGap` = clear air between edges after landing. Default: `0.2`

### Maximum follow-up distance (re-hit rhythm)
```
maxFollowupDistance = targetFollowupMaxDistance - rehitMargin
                    = max(rawMax, safeMinDistance)   // cannot be less than safeMin
```
- `targetFollowupMaxDistance` ≈ PlayerCombat.attackRange. Default: `2.0`
- `rehitMargin` keeps landing slightly inside attack range. Default: `0.15`

### Mirrored clamped desired distance
```
currentDist  = abs(enemyX - playerX)          // measured at hit moment
desiredDist  = clamp(currentDist, safeMinDistance, maxFollowupDistance)
candidateX   = playerX ± desiredDist          // mirrored from player centre
```
The enemy maintains roughly the same engagement distance — just on the other side.

### Queue-aware final X (respects existing enemies on target side)

If switching to **Right** (`frontEnemyX` = closest right-side enemy):
```
validZone = [playerX + safeMin  …  frontEnemyX - minSpacingOnTargetSide]
candidateX = clamp(candidateX, validZone)
```
If there is no room: land at `playerX + safeMin` (lane blocking pushes the front enemy outward naturally).

If switching to **Left** (`frontEnemyX` = closest left-side enemy):
```
validZone = [frontEnemyX + minSpacingOnTargetSide  …  playerX - safeMin]
candidateX = clamp(candidateX, validZone)
```

### Post-landing grace period
```
noGameOverUntil = Time.time + postSwitchNoGameOverDuration
```
`OnTriggerEnter2D` skips Game Over if `Time.time < noGameOverUntil`. Covers any residual collider contact at the landing frame.

---

## 3. New Tuning Fields

| Field | Default | Effect |
|-------|---------|--------|
| `sideSwitchGap` | `0.2` | Clear air gap between edges after landing. Increase if enemy still feels too close. |
| `rehitMargin` | `0.15` | Landing stays this far inside attack range. Increase if second hits sometimes miss. |
| `postSwitchNoGameOverDuration` | `0.06` | Post-landing grace period (seconds). Increase if one-frame overlap still triggers Game Over. |
| `minSpacingOnTargetSide` | `0.7` | Min gap between SwitchEnemy and existing target-side enemies. Increase if visual overlap occurs. |
| `targetFollowupMaxDistance` | `2.0` | Set equal to `PlayerCombat.attackRange`. Landing will always be reachable from here. |
| `sideSwitchDuration` | `0.08` | Crossing animation speed. Increase for slower, more readable switch; decrease for snappier feel. |

---

## 4. Helper Methods Added

| Method | Purpose |
|--------|---------|
| `GetHalfWidth()` | Enemy X extents from Collider2D / SpriteRenderer / fallback |
| `GetPlayerHalfWidth()` | Player X extents same way |
| `GetCurrentCenterDistanceToPlayer()` | abs(enemyX − playerX) at hit moment |
| `CalculateSafeMinSwitchDistance()` | sideSwitchGap + both half-widths |
| `CalculateMaxFollowupDistance()` | targetFollowupMaxDistance − rehitMargin (≥ safeMin) |
| `FindClosestFrontEnemyOnSide(side)` | Closest alive, non-self, non-switching enemy on that side |
| `CalculateQueueAwareSwitchTargetX(newSide)` | Full mirrored + clamped + queue-aware landing X |

---

## 5. Inspector Checklist

### SwitchEnemy prefab
- [ ] `behaviorType` = `SwitchSideOnHit`
- [ ] `maxHealth` = `2`
- [ ] `scoreValue` = `250`
- [ ] `moveSpeed` = `2.5`
- [ ] `sideSwitchGap` = `0.2`
- [ ] `rehitMargin` = `0.15`
- [ ] `postSwitchNoGameOverDuration` = `0.06`
- [ ] `minSpacingOnTargetSide` = `0.7`
- [ ] `targetFollowupMaxDistance` = `2.0` ← match PlayerCombat.attackRange
- [ ] `sideSwitchDuration` = `0.08`
- [ ] `enableLaneBlocking` = ✓
- [ ] `minSpacingFromFrontEnemy` = `0.8`
- [ ] `logPatternActions` = ✗ (enable to debug placement)
- [ ] **Collider2D present** — required for accurate half-width calculation

### EnemySpawner
- [ ] `enemyPrefabs[0]` = NormalEnemy
- [ ] `enemyPrefabs[1]` = HeavyEnemy
- [ ] `enemyPrefabs[2]` = SwitchEnemy

### Everything else (unchanged)
- [ ] `PlayerCombat → attackRange` still set (~2.0)
- [ ] `GameUI` references intact
- [ ] `Main Camera` has `CameraFollow` + `CameraShake`
- [ ] `Player` has `PlayerMovement` + `PlayerCombat`

---

## 6. How to Test

### Test 1 – Landing within follow-up range (from left)
1. Wait for SwitchEnemy to approach from the left at medium distance (~1.5–2 units away).
2. Attack left → combo+1, no score.
3. ✅ SwitchEnemy crosses to right side.
4. ✅ Landing X is approximately `playerX + (clamped approach distance)`.
5. ✅ Attack right immediately connects → combo+1, score +250.

### Test 2 – Landing within follow-up range (from right)
1. Same test from the right side.
2. ✅ Landing X is approximately `playerX - approach distance`.
3. ✅ Second hit connects without needing to wait.

### Test 3 – Queue awareness: enemy already on destination side
1. Let a NormalEnemy be close on the right side.
2. Let SwitchEnemy approach from the left.
3. Attack left → SwitchEnemy tries to cross to right.
4. ✅ SwitchEnemy lands **behind** the NormalEnemy with at least `minSpacingOnTargetSide` gap.
5. ✅ No visual overlap.
6. Kill NormalEnemy → SwitchEnemy moves to front.
7. Attack right → SwitchEnemy defeated.

### Test 4 – No unfair Game Over during crossing
1. Repeat tests 1 and 2 multiple times.
2. ✅ Game Over NEVER fires during the crossing animation.
3. ✅ Game Over NEVER fires during the `postSwitchNoGameOverDuration` grace window.
4. ✅ Game Over CAN fire after the grace period if the enemy naturally reaches the player.

### Test 5 – Second-hit rhythm (enable `logPatternActions`)
1. Check Console output after switching:
```
[SwitchEnemy(Clone)] Switching Left → Right. safeMin=0.90 maxFollowup=1.85
[SwitchEnemy(Clone)] Landed on Right at X=1.62. Grace until 12.34.
```
2. Confirm `X` is between `playerX + safeMin` and `playerX + maxFollowup`.

---

## 7. Tuning Guide

| Symptom | Adjustment |
|---------|-----------|
| Second hit still misses | Decrease `rehitMargin` (allows landing further out) |
| Enemy lands inside player | Increase `sideSwitchGap` |
| Game Over fires right after landing | Increase `postSwitchNoGameOverDuration` |
| SwitchEnemy overlaps queue on target side | Increase `minSpacingOnTargetSide` |
| Enemy lands too close for rhythm | Decrease `sideSwitchGap` (if currently high) |
| Crossing feels too fast/slow | Adjust `sideSwitchDuration` |

---

## 8. Known Limitations

- `FindClosestFrontEnemyOnSide` scans all enemies every switch — acceptable for a prototype with <10 enemies; use a per-side list for larger counts.
- If the collision system misses `OnTriggerEnter2D` at landing (due to fast movement), increase `postSwitchNoGameOverDuration` to `0.1`.
- No visual flash during the crossing — the movement is the only feedback.
- Spawn selection remains equal-chance random; no weights yet.
