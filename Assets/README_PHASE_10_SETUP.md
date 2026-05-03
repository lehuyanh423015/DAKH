# Phase 10 – Player Shift After Attack Setup Guide

## 1. What Was Added in Phase 10

| Feature | Details |
|---------|---------|
| **Attack-momentum shift** | Each accepted attack input moves the player slightly in the attack direction. |
| **Bounded combat zone** | Player X is clamped to `[minX, maxX]` so they cannot drift off screen. |
| **Smooth-step lerp** | Movement uses a smooth-step curve over `shiftDuration` seconds — no teleporting. |
| **Interrupt-safe** | If a new attack arrives before the previous shift finishes, the old coroutine is cancelled and a new one starts from the current position. |
| **No free WASD movement** | `PlayerMovement` never reads input directly; all shifts are triggered by `PlayerCombat`. |
| **Auto-detect** | `PlayerCombat` finds `PlayerMovement` on the same GameObject via `GetComponent` if not manually assigned. |

---

## 2. Files Created / Changed

### `PlayerMovement.cs` — **created** (`Assets/Scripts/Player/PlayerMovement.cs`)
New script. Responsible for:
- `ShiftLeft()` / `ShiftRight()` / `ShiftByDirection(int)` public API.
- Smooth-step coroutine from current X to clamped target X.
- `[minX, maxX]` boundary enforcement.
- Optional `logMovement` debug flag.

### `PlayerCombat.cs` — **updated** (minimal changes)
- New `[Header("Player Movement")] [SerializeField] private PlayerMovement playerMovement;` field.
- `Awake()` auto-detect: `playerMovement = GetComponent<PlayerMovement>()` if slot is empty.
- `Update()` left-attack block: `playerMovement?.ShiftLeft()` before `TryAttack`.
- `Update()` right-attack block: `playerMovement?.ShiftRight()` before `TryAttack`.
- All other code unchanged.

### `Enemy.cs` — **no changes**
Already tracks `playerTransform.position` live every frame — enemies correctly follow the shifted player.

### All other scripts — **no changes**
`GameManager.cs`, `EnemySpawner.cs`, `GameUI.cs`, `DifficultyManager.cs`, `CameraShake.cs`, `TemporaryEffect.cs`

---

## 3. Important Design Explanation

### Player has no free movement
`PlayerMovement` has **no input reading**. The only way the player moves is when `PlayerCombat` calls `ShiftLeft()` or `ShiftRight()`, which happens exactly on accepted attack key presses. Stun and game-over guards in `PlayerCombat.Update()` prevent any shift from happening during those states.

### Why shift happens before `TryAttack`
`ShiftByDirection` starts a coroutine but the actual position change is spread over `shiftDuration` (0.08 s). `TryAttack` runs synchronously on the same frame, so it reads the position **before** the shift moves the player. This is intentional — the attack uses the *pre-shift* position (the enemy was targeted before the momentum kick), which is the correct behaviour. If you prefer the hit to be checked from the *post*-shift position, swap the call order.

### Attack range stays relative to current position
`TryAttack` always reads `transform.position.x` at call time. After the shift animation, the player's position changes, but the combat range check on the *next* attack will use that new position. This is correct and requires no extra code.

### Enemy tracking
Enemies call `(playerTransform.position - transform.position).normalized` every frame. As the player drifts, enemies naturally adjust their approach angle each `Update()`.

---

## 4. Inspector Checklist

### Player GameObject
- [ ] `PlayerMovement.cs` is attached.
- [ ] `PlayerCombat.cs` is attached.
- [ ] `PlayerCombat → Player Movement` slot is assigned **or** left empty for auto-detect.

### PlayerMovement values (update manually — Unity won't auto-apply script defaults to existing objects)
- [ ] `Shift Distance` = `0.25`
- [ ] `Shift Duration` = `0.08`
- [ ] `Min X` = `-2.5`
- [ ] `Max X` = `2.5`
- [ ] `Log Movement` = ✗ (unchecked)

### Carry-over (unchanged from Phase 9)
- [ ] `PlayerCombat → attackRange` still set (~2.0)
- [ ] `PlayerCombat → stunDuration` still set (~0.4)
- [ ] Phase 4 feedback slots still assigned (indicators, hitEffectPrefab, cameraShake)
- [ ] `EnemySpawner → enemyPrefabs` still assigned (NormalEnemy + HeavyEnemy)
- [ ] `EnemySpawner → Difficulty Manager` still assigned
- [ ] `GameUI` references still assigned
- [ ] Both enemy prefabs have `enableLaneBlocking = true`

---

## 5. How to Test – Left / Right Shift

1. Press **Play**.
2. Press **A** or **Left Arrow**.
3. ✅ Player visually slides slightly left (~0.25 units) over 0.08 s.
4. Press **D** or **Right Arrow**.
5. ✅ Player visually slides slightly right.
6. Press A many times rapidly.
7. ✅ Player reaches `minX = -2.5` and stops shifting further left.
8. Press D many times rapidly.
9. ✅ Player reaches `maxX = 2.5` and stops shifting further right.
10. Enable `logMovement = true` to see `"Player shifted left."` / `"Player shifted right."` in Console.

---

## 6. How to Test – Attack Detection After Shift

1. Press A several times to drift left.
2. Let a NormalEnemy approach from the left.
3. ✅ Attack detection still finds the enemy because `TryAttack` uses current `transform.position`.
4. Drift right and repeat with a right-side enemy.
5. ✅ Hit/miss logic unchanged from the player's current position.

---

## 7. How to Test – Stun / Game Over Restrictions

1. Miss an attack to enter stun.
2. Press A or D during stun.
3. ✅ **No shift** and **no attack** — both are blocked by `isStunned` guard in `PlayerCombat.Update()`.
4. Recover from stun (wait `stunDuration` seconds).
5. ✅ Attacks and shifts resume normally.
6. Let an enemy reach the player → Game Over.
7. Press A or D after Game Over.
8. ✅ No shift, no attack — blocked by `IsGameOver` guard.

---

## 8. Regression Test – All Previous Features

| Feature | Expected result |
|---------|----------------|
| NormalEnemy (1 hit) | Dies on first correct hit; combo+1; score increases |
| HeavyEnemy (2 hits) | Survives first hit (knockback), dies on second; score increases then |
| Lane blocking | Same-side enemies queue with spacing; auto-resume after front death |
| Combo on every valid hit | Increases on lethal and non-lethal hits |
| Combo timeout | Resets after `comboWindowDuration` with no hit |
| Miss stun | Stun, tint, camera shake; no shift during stun |
| Score | Only increases on enemy defeat |
| Attack indicators | Flash on key press regardless of hit/miss |
| Hit effect | Spawns on both lethal and non-lethal hits |
| Camera shake | Miss = light, game over = strong |
| Difficulty scaling | Speed and interval increase over time |
| Enemy movement | All enemies follow player's current position |
| Game Over panel | Appears when any enemy touches player |
| Restart | Scene reloads; position, score, combo, difficulty all reset |

---

## 9. Known Limitations

- ❌ Camera does not follow the player yet — player can drift left/right of centre while camera stays fixed.
- ❌ Fixed spawn positions (`leftSpawnPosition`, `rightSpawnPosition`) do not adjust with player shift.
- ❌ No movement animation — shift is purely transform lerp.
- ❌ No player dash/dodge system — shift is purely game feel momentum.
- ❌ No camera framing adjustment yet.

---

## 10. Next Phase – Phase 11 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Smooth camera follow (X axis)** | Each frame: `cam.position.x = Mathf.Lerp(cam.position.x, player.position.x, cameraLag * Time.deltaTime)`. |
| **Camera dead zone** | Only follow when player X deviates beyond a threshold (e.g. 0.5 units from camera centre). |
| **Spawn position follow** | Offset `leftSpawnPosition` and `rightSpawnPosition` relative to camera/player centre so enemies always enter from the screen edges. |
| **Camera bounds** | Clamp camera X to `[minCamX, maxCamX]` matching the combat zone. |
