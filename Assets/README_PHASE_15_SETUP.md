# Phase 15 – Combo Shield & Miss Forgiveness Setup Guide

## 1. What Was Added in Phase 15

| Feature | Details |
|---------|---------|
| **Combo Shield** | Players earn a shield by reaching a configured combo threshold (default: 10). |
| **Miss Forgiveness** | If the player misses an attack while the shield is active, the shield is consumed, but the player is **not** stunned and there is no camera shake. |
| **Combo Reset** | A shielded miss *still* resets the combo to 0. This preserves the risk of long patterns without the punishing frustration of being stunned. |
| **Shield UI** | `GameUI` now listens to `GameManager.OnComboShieldChanged` and displays the shield status on-screen. |

---

## 2. Files Created / Changed

### `GameManager.cs` — **updated**
- Added `comboShieldThreshold` and `maxComboShields` settings.
- Added `TryConsumeComboShield()` to safely deduct a shield when requested.
- Automatically grants a shield in `RegisterSuccessfulHit()` if the threshold is met.
- Fires `OnComboShieldChanged` for the UI.

### `PlayerCombat.cs` — **updated**
- Modified the miss logic to call `GameManager.Instance.TryConsumeComboShield()`.
- If a shield is consumed, the player skips the stun, camera shake, and red tint routines.
- Added an optional `shieldBreakEffectPrefab` to instantiate when a shield breaks.

### `GameUI.cs` — **updated**
- Added a `shieldText` field.
- Subscribes to `OnComboShieldChanged`.
- Shows "Shield: READY" when a shield is active, and "Shield: -" when empty.

---

## 3. Important Design Explanation

### Why a Combo Shield?
As enemy patterns get more complex (like Phase 13's 3-hit alternating enemy), one mistimed button press can feel overly punishing if it results in a stun. The combo shield reduces this frustration by "forgiving" the stun for high-performing players.
However, it still **resets the combo to 0**. This ensures that high scores require true perfection, while casual survival is slightly more forgiving.

---

## 4. Inspector Checklist

### GameManager
Select the **GameManager** GameObject:
- [ ] `comboShieldThreshold` = **10**
- [ ] `maxComboShields` = **1**
- [ ] `resetComboOnShieldedMiss` = **True**

### Canvas & GameUI
1. In your **Canvas**, duplicate the existing `ComboText` or create a new TextMeshPro element.
2. Name it **ShieldText**.
3. Position it clearly (e.g. Anchor: top-left, Pos X: 120, Pos Y: -110).
4. Default text: `Shield: -`
5. Select the GameObject with **GameUI.cs**.
- [ ] `shieldText` = **ShieldText** (drag the newly created UI element here).
- [ ] Existing `scoreText`, `comboText`, `gameOverPanel` are still assigned.

### PlayerCombat
Select your **Player** GameObject:
- [ ] (Optional) Create a simple break effect prefab and assign it to `shieldBreakEffectPrefab`.
- [ ] Existing combat/feedback fields are still assigned.

---

## 5. How to Test Shield Gain
1. Press Play.
2. Successfully hit 10 consecutive times without missing.
3. Check the Console: it should log `"Combo Shield gained!"`
4. Look at the HUD: it should display **Shield: READY**.

## 6. How to Test Shielded Miss
1. After gaining the shield, press an attack button with no enemy in range to intentionally miss.
2. **Observe:** The combo resets to 0.
3. **Observe:** You are **NOT** stunned. You can immediately attack again.
4. **Observe:** There is no camera shake.
5. Look at the HUD: it should display **Shield: -**.
6. Check the Console: it should log `"Miss forgiven by Combo Shield."`

## 7. How to Test Normal Miss
1. Immediately miss again (while the shield is empty).
2. **Observe:** You are stunned (tinted red), and the camera shakes.

## 8. How to Test Shield Reacquire
1. After losing the shield, successfully build your combo back up to 10.
2. **Observe:** You gain the shield again.

---

## 9. Regression Test

| Feature | Expected Result |
|---------|----------------|
| **NormalEnemy** | Dies in 1 hit |
| **HeavyEnemy** | Knockback on hit 1, dies on hit 2 |
| **SwitchEnemy** | Switches side on hit 1, dies on hit 2 |
| **PatternEnemy3Hit**| Alternates sides on hits 1 & 2, dies on hit 3 |
| **Weighted Spawning** | Normal enemies spawn more often |
| **Lane Blocking** | Enemies queue up correctly on the same side |
| **Player Shift** | Player subtly shifts after attacks |
| **Camera Follow** | Smoothly tracks player X position |
| **Score** | Score only increases on enemy defeat |
| **Combo Timeout** | Waiting too long resets combo |

---

## 10. Known Limitations

- **Stun Only:** The shield only protects against stuns from missing. Touching an enemy is still an instant Game Over.
- **No Animations:** Shield UI is text-only. There is no flashing animation or sound effect yet.
- **Max Shields:** The system supports `maxComboShields`, but the UI text only shows "READY" vs "-". It doesn't show a number (e.g. "Shields: 2"). For a prototype, max = 1 is best.

---

## 11. Next Phase Suggestion

**Phase 16:**
- Implement **Sound Effects** (AudioSource on Player/Enemies, audio clips for Hit, Miss, Shield Break, Stun, Game Over).
- OR implement **Visual Polish** (Combo number popping animation, Shield UI flashing when ready).
