# Phase 18 – Visual & UI Polish Setup Guide

## 1. What Was Added in Phase 18

| Feature | Details |
|---------|---------|
| **UI Pop Effect** | A simple script (`UIPopEffect`) that scales UI elements up and smoothly returns them to their original size when important values (like Score or Combo) change. |
| **UI Flash Effect** | A simple script (`UIFlashEffect`) that temporarily changes the color of a TextMeshPro text element (e.g. Shield ready/consumed events). |
| **Game Over Panel Guidance** | Layout recommendations for better readability on game over. |
| **Survival Time Display** | Integrated the `finalTimeText` to visibly show your session time inside the Game Over panel. |
| **Background Markers** | A new script (`BackgroundMarkerGenerator`) that generates vertical lines to help ground the player and make camera tracking movements obvious. |

---

## 2. Files Created / Changed

### Created
- `Assets/Scripts/UI/UIPopEffect.cs`
- `Assets/Scripts/UI/UIFlashEffect.cs`
- `Assets/Scripts/Environment/BackgroundMarkerGenerator.cs`

### Updated
- `Assets/Scripts/UI/GameUI.cs` — Added references to the new effects and triggered them automatically when scores/combos/shields change.

---

## 3. Important Design Explanation

**This phase is pure polish.**
It does not change a single line of gameplay logic. All effects added in this phase are optional. If you forget to assign an effect in the inspector, the game continues to run without throwing errors. The visuals here are simple placeholders to improve game feel and readability, setting a foundation that can easily be replaced by real animations or particle systems later.

---

## 4. UI Setup Checklist

To enable the new UI feedback, perform the following in your Unity Editor:

### Add Effects to Text Elements
1. Select your **ScoreText** UI object.
   - Click **Add Component** -> `UIPopEffect`.
2. Select your **ComboText** UI object.
   - Click **Add Component** -> `UIPopEffect`.
3. Select your **ShieldText** UI object.
   - Click **Add Component** -> `UIFlashEffect`.

### Link Effects to GameUI
1. Select your **GameUI** object.
2. In the Inspector, locate the new **UI Effects (optional)** header.
3. Drag your **ScoreText** object into `Score Pop Effect`.
4. Drag your **ComboText** object into `Combo Pop Effect`.
5. Drag your **ShieldText** object into `Shield Flash Effect`.

*(Ensure your **GameOverPanel** contains a `FinalTimeText` and it is linked in the GameUI script).*

### Game Over Panel Readability
Ensure your Game Over Panel looks structured:
- [ ] Add an Image to the panel with a semi-transparent black color (`Alpha ~150`).
- [ ] Center the text layout vertically.
- [ ] Make the Title text large.
- [ ] Make the Restart Button prominent at the bottom.

---

## 5. Background Marker Setup Checklist

This creates a sense of space so that when the camera moves with the player, you can actually see the movement against the background.

1. Create an **Empty GameObject** in your MainScene. Name it `BackgroundMarkers`.
2. Attach the `BackgroundMarkerGenerator.cs` script.
3. In the Inspector, assign the `Marker Sprite` field (e.g., Unity's default Square sprite, or any plain white square).
4. Leave `Generate On Start` set to True.
5. Hit **Play**. 
6. Confirm that faint vertical lines appear behind the gameplay area.

*(If you don't assign a sprite, it will log a warning and safely skip generation).*

---

## 6. How to Test UI Pop

1. Press Play.
2. Hit an enemy.
3. Notice the **ComboText** slightly scale up and bounce back.
4. Defeat an enemy.
5. Notice the **ScoreText** slightly scale up and bounce back.

---

## 7. How to Test Shield Flash

1. Build your combo to the threshold (default 10).
2. Notice the **ShieldText** flash yellow (or your chosen color) when it says "READY".
3. Intentionally miss an attack while the shield is up.
4. Notice the **ShieldText** flash again as it returns to "-".

---

## 8. How to Test Game Over Panel

1. Let an enemy hit you.
2. The Game Over Panel should appear.
3. You should clearly see the **Final Score**.
4. You should clearly see the **Survival Time** (e.g., `Time: 12.34s`).
5. Click **Restart** to ensure the game reloads cleanly.

---

## 9. How to Replace Placeholders Later

- **Backgrounds:** The `BackgroundMarkers` object can be deleted once you have real background art (parallax layers, tilesets, etc.).
- **Characters:** When adding real player/enemy sprites, add them as **child GameObjects** under the main Player/Enemy objects. Keep the Rigidbody2D and Collider2D on the parent object.
- **UI:** The pop and flash scripts can be replaced later with Unity Animators or DOTween if more complex sequencing is needed.

---

## 10. Regression Test

- [x] All enemy types (Normal, Heavy, Switch, Pattern3) function normally.
- [x] Spawning and lane blocking are unaffected.
- [x] Combat logic, misses, stuns, and shields behave identically to Phase 17.
- [x] Camera smoothly tracks the player.
- [x] Sound effects play successfully.

---

## 11. Known Limitations

- **Effects are simple:** The pop/flash effects use Coroutines and `Vector3.Lerp` / `Color.Lerp`. They are not complex animation curves.
- **Background is basic:** The markers are just stretched sprites, not a cohesive environment.
- **No real art yet:** Everything is still placeholder.

---

## 12. Next Phase Suggestion

**Phase 19:** Asset Replacement Plan.
- Begin importing actual game sprites.
- Setup animation controllers for the Player (Idle, Attack Left, Attack Right, Stunned).
- Replace enemy blocks with animated prefabs.
- Swap out the hit effects and background markers with real visual assets.
