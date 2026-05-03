# Phase 4 – Game Feel & Visual Feedback Setup Guide

## 1. What Was Added in Phase 4

| Feature | Details |
|---------|---------|
| **Attack direction indicator** | A sprite (left or right) flashes briefly near the player each time an attack key is pressed. |
| **Hit feedback** | An optional prefab (`HitEffect`) is instantiated at the destroyed enemy's position for ~0.15 s. |
| **Miss / stun color tint** | The player sprite turns red while stunned and returns to white on recovery. |
| **Camera shake** | Light shake on miss; stronger shake on game over. Both are optional and data-driven. |

---

## 2. Files Created / Changed

### `PlayerCombat.cs` — **updated**
All Phase 1 and 2 logic preserved. Phase 4 additions:
- New `[Header("Attack Indicators")]` fields: `leftAttackIndicator`, `rightAttackIndicator`, `attackIndicatorDuration`.
- New `[Header("Hit Effect")]` field: `hitEffectPrefab`.
- New `[Header("Stun Visual")]` fields: `playerSpriteRenderer` (auto-detected), `stunColor`, `normalColor`.
- New `[Header("Camera Shake")]` fields: `cameraShake`, shake durations and strengths.
- `ShowAttackIndicator()` + `IndicatorRoutine()` coroutine.
- `SpawnHitEffect()` helper.
- `StunRoutine()` extended with sprite color tint.
- `HandleGameOver()` subscribed to `GameManager.OnGameOverEvent` for game-over shake.

### `CameraShake.cs` — **created** (`Assets/Scripts/Feedback/CameraShake.cs`)
Attach to Main Camera. Exposes `Shake(duration, strength)`. Coroutine-based, restores camera position after each shake.

### `TemporaryEffect.cs` — **created** (`Assets/Scripts/Feedback/TemporaryEffect.cs`)
Self-destructs after a configurable `lifetime`. Attach to the `HitEffect` prefab.

### `GameManager.cs` — **no changes**
`OnGameOverEvent` (added in Phase 3) is used by `PlayerCombat` directly. No edits needed.

### `Enemy.cs` — **no changes**

### `EnemySpawner.cs` — **no changes**

### `GameUI.cs` — **no changes**

---

## 3. Required / Optional Scene Hierarchy

```
MainScene
├── GameManager                 (existing)
├── EnemySpawner                (existing)
├── Player                      (existing)
│   ├── LeftAttackIndicator     ← NEW optional child GameObject
│   └── RightAttackIndicator    ← NEW optional child GameObject
├── Main Camera                 ← attach CameraShake.cs here
├── Canvas                      (existing)
└── GameUI                      (existing)

Assets/Prefabs/
├── Enemy.prefab                (existing)
└── HitEffect.prefab            ← NEW optional prefab
```

---

## 4. Step-by-Step Scene Setup

### A — Attack Indicators (optional but recommended)

1. Select the **Player** GameObject in the Hierarchy.
2. Add two child GameObjects: **LeftAttackIndicator** and **RightAttackIndicator**.
   - Right-click Player → Create Empty → rename each.
3. Add a **Sprite Renderer** to each (Inspector → Add Component → Sprite Renderer).
   - Assign any sprite (a small square/circle from your Art folder, or the default Unity square).
   - Set a visible color (e.g. yellow for left, cyan for right).
4. Position them relative to the player:
   - `LeftAttackIndicator`  local position: `(-1, 0, 0)` or similar.
   - `RightAttackIndicator` local position: `( 1, 0, 0)` or similar.
5. **Deactivate both** in the Inspector (uncheck the checkbox next to their names).
6. Select the **Player** → in `PlayerCombat` component:
   - Drag `LeftAttackIndicator`  → `Left Attack Indicator` slot.
   - Drag `RightAttackIndicator` → `Right Attack Indicator` slot.

### B — Hit Effect Prefab (optional)

1. In the Hierarchy, create a new **Sprite** GameObject (right-click → 2D Object → Sprite).
2. Set the **Sprite Renderer** sprite to any small sprite; color it yellow/white.
3. Set scale to something small, e.g. `(0.3, 0.3, 1)`.
4. Add `TemporaryEffect.cs` to it (Inspector → Add Component).
5. Set `Lifetime` to `0.15`.
6. Drag this object into `Assets/Prefabs/` to create **HitEffect.prefab**.
7. Delete the scene version (it's now a prefab).
8. Select the **Player** → in `PlayerCombat` → drag `HitEffect.prefab` → `Hit Effect Prefab` slot.

### C — Camera Shake (optional but recommended)

1. Select **Main Camera** in the Hierarchy.
2. Add Component → `CameraShake`.
3. Select the **Player** → in `PlayerCombat`:
   - Drag the **Main Camera** into the `Camera Shake` slot.
   - (The component reference is automatically resolved to `CameraShake`.)

### D — Player Sprite Renderer (auto-detected)

The script calls `GetComponent<SpriteRenderer>()` automatically.
If your player sprite is on a child object instead of the root:
- Drag that child's `SpriteRenderer` into the `Player Sprite Renderer` slot manually.

---

## 5. Inspector Checklist

### PlayerCombat (on Player)
- [ ] `attackRange` is set (e.g. `2.0`). *(Phase 1)*
- [ ] `stunDuration` is set (e.g. `0.4`). *(Phase 2)*
- [ ] `playerSpriteRenderer` is assigned or auto-detected. *(Phase 4)*
- [ ] `stunColor` is set (default red). *(Phase 4)*
- [ ] `normalColor` is set (default white). *(Phase 4)*
- [ ] `leftAttackIndicator` assigned if used. *(Phase 4)*
- [ ] `rightAttackIndicator` assigned if used. *(Phase 4)*
- [ ] `attackIndicatorDuration` is set (default `0.15`). *(Phase 4)*
- [ ] `hitEffectPrefab` assigned if used. *(Phase 4)*
- [ ] `cameraShake` assigned if used. *(Phase 4)*

### Main Camera
- [ ] `CameraShake.cs` is attached to the Main Camera (if shake is desired).

### Existing checklist (still required)
- [ ] `GameManager.cs` attached to GameManager object.
- [ ] `EnemySpawner` has Enemy Prefab and Player Transform assigned.
- [ ] `GameUI.cs` has all five UI references assigned.
- [ ] Player tag is `"Player"`.
- [ ] Player Box Collider 2D → Is Trigger = ✓.
- [ ] Enemy Box Collider 2D → Is Trigger = ✗.
- [ ] Enemy Rigidbody 2D → Body Type = Kinematic.

---

## 6. How to Test – Attack Indicator

1. Press **Play**.
2. Press **A** or **Left Arrow** (even when no enemy is close).
3. ✅ `LeftAttackIndicator` sprite briefly appears (≈0.15 s) then hides.
4. Press **D** or **Right Arrow**.
5. ✅ `RightAttackIndicator` sprite briefly appears then hides.

---

## 7. How to Test – Hit Feedback

1. Press Play. Wait for an enemy to come within `attackRange`.
2. Press the correct direction key to kill it.
3. ✅ A small `HitEffect` sprite appears at the enemy's last position for ≈0.15 s.
4. ✅ Enemy is destroyed as before.
5. ✅ Score / combo update in UI as before.

---

## 8. How to Test – Miss / Stun Visual

1. Press an attack key when no enemy is within range.
2. ✅ Player sprite changes to the **stun color** (red by default).
3. ✅ Pressing attack again during stun does nothing.
4. After `stunDuration` (0.4 s):
5. ✅ Player sprite returns to **normal color** (white).
6. ✅ Console prints `Player stunned.` → `Player recovered.`
7. ✅ Combo resets to 0 in the UI.

---

## 9. How to Test – Camera Shake

1. Miss an attack.
2. ✅ Camera moves slightly for ≈0.15 s then returns to position.
3. Let an enemy reach the player (game over).
4. ✅ Camera shakes with more intensity for ≈0.25 s.
5. If `CameraShake` is not assigned to `PlayerCombat`, no shake occurs — gameplay is unaffected.

---

## 10. Regression Test – All Previous Features

After setting up Phase 4, verify nothing is broken:

| Feature | Expected result |
|---------|----------------|
| Enemy spawn | Enemies appear from left/right every ~1.5 s |
| Enemy movement | Enemies move toward player and stop on game over |
| Hit destroys enemy | Correct-side attack within range kills the closest enemy |
| Score UI updates | `scoreText` increments after each kill |
| Combo UI updates | `comboText` increments per kill, resets on miss |
| Miss resets combo | Pressing wrong direction / out of range resets combo to 0 |
| Game Over panel | Panel appears when enemy touches player |
| Final score shown | `finalScoreText` shows correct score |
| Restart button | Scene reloads and everything resets |
| Console logs | Hit / Miss / Stun / Game Over logs still appear |

---

## 11. Known Limitations

- ❌ Feedback is simple placeholder visuals — no polished animations.
- ❌ No sound effects yet.
- ❌ No particle systems (just static sprite flash for hit effect).
- ❌ No difficulty scaling yet.
- ❌ No multiple enemy types yet.
- ❌ No combo visual feedback (text only).
- ⚠️ Hit effect requires a prefab with a sprite to be visible — empty prefabs will "work" but show nothing.

---

## 12. Next Phase – Phase 5 Suggestions

| Feature | Notes |
|---------|-------|
| **Difficulty scaling** | Gradually increase `moveSpeed` and reduce `spawnInterval` over time in `EnemySpawner`. |
| **Enemy variety** | Add a second enemy type with different speed / score value. |
| **Sound effects** | `AudioSource.PlayOneShot()` for hit, miss, and game over. |
| **Combo visual** | Flash the combo text or scale it briefly on increment. |
| **UI polish** | Animate the Game Over panel in (lerp alpha or slide). |
| **Better pacing** | Add waves or a timer-based survival mode. |
