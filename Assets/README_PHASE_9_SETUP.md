# Phase 9 – Lane Blocking / Enemy Queue Setup Guide

## 1. What Was Added in Phase 9

| Feature | Details |
|---------|---------|
| **Lane blocking** | Back-of-queue enemies pause when the enemy ahead is too close, preventing overlap and slipping. |
| **Same-side queue** | Each side forms an implicit queue: only the frontmost enemy approaches freely; those behind it stack up with a gap. |
| **Knockback-safe blocking** | A knocked-back HeavyEnemy still counts as a front enemy, so enemies behind it correctly wait during knockback. |
| **Auto-resume** | Once the front enemy is destroyed, the gap check fails immediately and the next enemy resumes moving — no deadlock. |
| **Per-prefab tuning** | `enableLaneBlocking` and `minSpacingFromFrontEnemy` are serialized and tunable per prefab. |
| **Optional debug log** | `logLaneBlocking` prints once per blocking event (throttled — not every frame). |

---

## 2. Files Created / Changed

### `Enemy.cs` — **updated**
New serialized fields (Phase 9):
- `bool enableLaneBlocking = true` — master switch for this enemy.
- `float minSpacingFromFrontEnemy = 0.8f` — minimum gap to keep from the enemy ahead.
- `bool logLaneBlocking = false` — optional debug logging.

New private state:
- `bool wasBlockedLastFrame` — throttles log so it fires once per block event, not every frame.

New private methods:
- `bool IsBlockedByFrontEnemy()` — scans all same-side non-defeated enemies; returns true if the nearest one in front is within `minSpacingFromFrontEnemy`.
- `bool IsSameSideEnemyInFront(float otherX, float myX)` — direction-aware "is this enemy closer to the player than me?" check.
- `float DistanceTo(Enemy other)` — simple 2D distance helper.

`Update()` change:
- After `isKnockedBack` check, calls `IsBlockedByFrontEnemy()`. If blocked, skips movement for that frame.

All other methods (Initialize, SetMoveSpeed, TakeHit, KnockbackRoutine) — **unchanged**.

### `PlayerCombat.cs` — **no changes**
Already selects the *closest* valid enemy on the targeted side within `attackRange`. This is exactly the correct Phase 9 behaviour — the player always hits the frontmost reachable enemy.

### `EnemySpawner.cs` — **no changes**
### `GameManager.cs` — **no changes**
### `GameUI.cs` — **no changes**
### `DifficultyManager.cs` — **no changes**

---

## 3. Important Design Explanation

### How lane blocking works

```
[spawn edge]  Enemy B ─── gap ─── Enemy A ───► Player
```

- **Enemy A** (front): moves freely toward the player.
- **Enemy B** (back): checks the gap to A each frame. If `gap < minSpacingFromFrontEnemy`, Enemy B stops for that frame. Otherwise it moves normally.
- When Enemy A is destroyed, Enemy B's gap check fails (no front enemy), so it immediately resumes.

### Why opposite-side enemies are unaffected

`IsSameSideEnemyInFront` only considers enemies with the same `Side` enum value. A left-side enemy never blocks a right-side enemy and vice versa.

### Knockback interaction

When HeavyEnemy is knocked back 0.45 units:
- `isKnockedBack = true` → HeavyEnemy's own movement stops.
- Enemy B's gap check still sees HeavyEnemy in its path → Enemy B pauses too.
- After 0.08 s, knockback ends → HeavyEnemy resumes → gap re-opens → Enemy B resumes when gap exceeds threshold.

### No deadlock guarantee

`IsBlockedByFrontEnemy()` scans live objects every frame. As soon as the front enemy is destroyed (removed from the scene), it disappears from `FindObjectsByType` and can no longer block. Enemy B resumes on the very next `Update()` call.

---

## 4. Inspector Checklist

### Both enemy prefabs (update manually — Unity serialization)
- [ ] `NormalEnemy` → `Enable Lane Blocking` = ✓ (checked)
- [ ] `NormalEnemy` → `Min Spacing From Front Enemy` = `0.8`
- [ ] `NormalEnemy` → `Log Lane Blocking` = ✗ (unchecked)
- [ ] `HeavyEnemy`  → `Enable Lane Blocking` = ✓
- [ ] `HeavyEnemy`  → `Min Spacing From Front Enemy` = `0.8`
- [ ] `HeavyEnemy`  → `Log Lane Blocking` = ✗

### Carry-over (unchanged from Phase 8)
- [ ] `EnemySpawner → enemyPrefabs[0]` = NormalEnemy
- [ ] `EnemySpawner → enemyPrefabs[1]` = HeavyEnemy
- [ ] `EnemySpawner → Player Transform` still assigned
- [ ] `EnemySpawner → Difficulty Manager` still assigned
- [ ] `PlayerCombat → attackRange` still set (~2.0)
- [ ] `GameUI` references still assigned
- [ ] Phase 4 feedback slots still assigned

---

## 5. How to Test – Same-Side Blocking

1. Press **Play**.
2. Wait for two or more enemies to spawn from the **same side**.
3. ✅ The front enemy moves toward the player normally.
4. ✅ The back enemy **slows to a stop** when the gap closes to ~0.8 units.
5. ✅ Enemies do **not** heavily overlap or pass through each other.
6. Destroy the front enemy (correct hit).
7. ✅ The back enemy **immediately resumes** moving toward the player.

Enable `logLaneBlocking = true` on one prefab to see:
```
[NormalEnemy] Lane blocked by front enemy.
```
(Appears once per blocking event, not every frame.)

---

## 6. How to Test – HeavyEnemy Blocking

1. Let a HeavyEnemy walk close to the player on one side.
2. Let a second enemy spawn behind it on the same side.
3. ✅ Second enemy stacks behind HeavyEnemy with a gap.
4. Hit HeavyEnemy once → it is knocked back 0.45 units.
5. ✅ Second enemy **does not rush through** — it stays blocked while HeavyEnemy is in the way.
6. Hit HeavyEnemy a second time → it is destroyed.
7. ✅ Second enemy now **resumes moving** toward the player.

---

## 7. How to Test – Player Targeting

1. Wait for two enemies to stack on the same side (Enemy A in front, Enemy B behind).
2. Attack that side.
3. ✅ The hit registers on **Enemy A** (closest to player), not Enemy B.
4. ✅ If Enemy A is a HeavyEnemy and survives, Enemy B is still not hit.
5. Kill Enemy A. Attack again.
6. ✅ Now Enemy B is the frontmost — it is hit next.

---

## 8. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| NormalEnemy | Dies in 1 hit; combo+1; score increases |
| HeavyEnemy | Survives hit 1 (knockback), dies on hit 2 |
| Lane blocking | Enemies queue, don't overlap, auto-resume on front enemy death |
| Enemy spawn | NormalEnemy and HeavyEnemy spawn randomly from left/right |
| Enemy movement | All enemies same speed; stops on game over |
| Difficulty scaling | Speed and interval increase over time |
| Combo on every hit | Lethal and non-lethal hits both count |
| Combo timeout | Resets after `comboWindowDuration` with no hit |
| Miss stun | Stun, tint, camera shake |
| Score | Only on enemy defeat |
| Attack indicators | Flash on key press |
| Hit effect | Both lethal and non-lethal |
| Camera shake | Miss = light, game over = strong |
| Game Over panel | Appears when enemy touches player |
| Restart | Scene reloads; all state resets |

---

## 9. Known Limitations

- ❌ Lane blocking uses `FindObjectsByType<Enemy>()` every frame — acceptable for a prototype; optimize with per-side lists for larger enemy counts.
- ❌ Enemies may visually touch slightly at the blocking boundary depending on scale vs. `minSpacingFromFrontEnemy`; tune the value to match your sprite sizes.
- ❌ Enemies on opposite sides are fully independent — no cross-side coordination.
- ❌ No pattern enemies yet (e.g. alternating side attacks).
- ❌ No combo shield / miss forgiveness yet.
- ❌ No player shift / camera follow yet.
- ❌ No spawn weights yet (50/50 random between Normal and Heavy).

---

## 10. Next Phase – Phase 10 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Player shift after attack** | After a successful hit, nudge the player slightly left/right (toward the attacked side). Clamp within a `[-maxShift, +maxShift]` range. |
| **Camera follow** | Smoothly lerp Main Camera X toward player X with a `cameraLag` parameter. |
| **Bounded combat zone** | Define `leftBound` and `rightBound` on Player; clamp position every frame. |
| **Attack range tied to position** | `attackRange` becomes relative to current player position so the shifted player can still reach enemies naturally. |
