# Phase 2 – Setup & Verification Guide

## 1. What Was Added in Phase 2

| Feature | Details |
|---------|---------|
| **Score** | Each enemy kill awards `baseScore + (combo × 10)` points. Accumulates across the session. |
| **Combo** | Increments with every consecutive kill. Resets to 0 on a miss or game over. |
| **Miss detection** | Pressing an attack key with no valid in-range enemy on that side counts as a miss. |
| **Stun after miss** | After a miss the player cannot attack for `stunDuration` seconds (default 0.4 s). |

---

## 2. Files Changed

### `GameManager.cs` — updated
Added `Score` and `Combo` properties and three new methods:
- `RegisterHit(baseScore)` — bumps combo, calculates gain, logs result.
- `RegisterMiss()` — resets combo, logs miss.
- `ResetCombo()` — utility reset without logging.
- `GameOver()` — now also logs `Final Score` and `Final Combo`.

### `PlayerCombat.cs` — updated
Added miss detection, `RegisterHit` / `RegisterMiss` calls, and the stun system:
- New serialized field `stunDuration` (default 0.4 s).
- Private bool `isStunned`.
- `StunRoutine()` coroutine that sets/clears `isStunned` and logs the stun window.
- Calls `enemy.ScoreValue` before destroying so the value is still readable.

### `Enemy.cs` — updated
Added scoring data:
- `[SerializeField] private int scoreValue = 100;`
- `public int ScoreValue => scoreValue;` — read by `PlayerCombat` on hit.

### `EnemySpawner.cs` — **no changes required**
Fully compatible with Phase 2. No modifications needed.

---

## 3. Inspector Checklist

Complete every item before pressing Play:

- [ ] `GameManager` object has `GameManager.cs` attached.
- [ ] `Player` object has `PlayerCombat.cs` attached.
- [ ] `PlayerCombat` → `attackRange` is set to **1.5 – 2.0**.
- [ ] `PlayerCombat` → `stunDuration` is set to **0.4**.
- [ ] `Enemy` prefab has `Enemy.cs` attached.
- [ ] `Enemy` prefab → `Enemy.cs` → `scoreValue` is set to **100**.
- [ ] `Enemy` prefab → `moveSpeed` is set to **2.0 – 3.0**.
- [ ] `EnemySpawner` still has **Enemy Prefab** assigned.
- [ ] `EnemySpawner` still has **Player Transform** assigned.
- [ ] Player GameObject has the tag **`Player`** set.
- [ ] Player Box Collider 2D → **Is Trigger = ✓ (enabled)**.
- [ ] Enemy Box Collider 2D → Is Trigger = ✗ (not a trigger).
- [ ] Enemy Rigidbody 2D → Body Type = **Kinematic**.

---

## 4. How to Test Score

1. Press Play.
2. Wait for an enemy to walk within attack range.
3. Press the correct direction key.
4. Check the Console — you should see:
   ```
   Hit! Score: 110, Combo: 1, Gain: 110
   ```
   *(First hit: base 100 + combo 1 × 10 = 110)*

---

## 5. How to Test Combo

1. Hit multiple enemies consecutively without missing.
2. Watch the Console:
   ```
   Hit! Score: 110, Combo: 1, Gain: 110
   Hit! Score: 240, Combo: 2, Gain: 120
   Hit! Score: 390, Combo: 3, Gain: 130
   ```
3. Combo increases with each kill; score gain grows accordingly.

---

## 6. How to Test Miss

1. Press an attack key when no enemy is within `attackRange` on that side  
   (or press the wrong direction while only the opposite side has an enemy).
2. Check the Console:
   ```
   Miss! Combo reset.
   ```
3. Confirm the combo counter resets to 0 on the next hit.

---

## 7. How to Test Stun

1. Miss an attack intentionally.
2. Check the Console:
   ```
   Player stunned.
   ```
3. Immediately press the attack key again — input should be **ignored** (no log, no action).
4. After 0.4 seconds, check the Console:
   ```
   Player recovered.
   ```
5. Now input works again.

---

## 8. How to Test Game Over

1. Let an enemy walk all the way into the player without attacking.
2. Check the Console:
   ```
   Game Over
   Final Score: 350
   Final Combo: 2
   ```
3. Confirm no new enemies spawn.
4. Confirm all existing enemies freeze in place.

---

## 9. Known Limitations

- ❌ No UI HUD (score/combo display on screen) yet.
- ❌ No Game Over UI panel yet.
- ❌ No restart button / restart flow yet.
- ❌ No animations yet.
- ❌ No sound / audio yet.
- ❌ No visual effects (hit flash, particle) yet.
- ❌ No difficulty scaling yet.
- ❌ No multiple enemy types yet.

---

## 10. Next Phase – Phase 3 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Score HUD** | Add a Canvas + TextMeshPro text; update it inside `RegisterHit`. |
| **Combo HUD** | Display current combo next to score; reset on miss. |
| **Game Over UI** | Show a panel with final score on `GameOver()`; hide during play. |
| **Restart flow** | Add a Restart button that calls `SceneManager.LoadScene(...)`. |
| **Hit visual feedback** | Flash enemy white for one frame using a `SpriteRenderer.color` coroutine. |
