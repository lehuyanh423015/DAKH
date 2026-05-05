# Phase 16 – Basic Audio Feedback Setup Guide

## 1. What Was Added in Phase 16

| Feature | Details |
|---------|---------|
| **AudioManager** | A new singleton script that centralizes simple `PlayOneShot` audio playback for all gameplay events. |
| **Gameplay Sound Hooks** | `PlayerCombat`, `GameManager`, and `GameUI` now call the `AudioManager` to play sounds at exact moments (hit, miss, stun, game over, shield events). |
| **Safe Execution** | If audio clips are missing or not assigned, the game continues to run without throwing errors. The manager can optionally log warnings to help with debugging. |

---

## 2. Files Created / Changed

### `AudioManager.cs` — **created**
- Centralized manager holding references to all essential sound effect clips.
- Provides public methods like `PlayHit()`, `PlayMiss()`, etc.
- Auto-detects or adds an `AudioSource` if one isn't explicitly assigned.

### `PlayerCombat.cs` — **updated**
- Added hooks for hit, defeat, miss, stun, and shield consumed sounds inside the combat logic.
- A shielded miss intentionally skips the stun sound and only plays the shield consumed sound.

### `GameManager.cs` — **updated**
- Added hooks for the game over sound and the combo shield gained sound.

### `GameUI.cs` — **updated**
- Added a hook for the restart button click sound.

---

## 3. Important Design Explanation

### Audio is Feedback Only
In this prototype, audio exists purely as secondary feedback. It does not control animation timings, dictate logic state, or block gameplay. The systems are designed to fire-and-forget (`PlayOneShot`). If you want to run the game completely silently or haven't sourced audio files yet, the game will function 100% identically.

---

## 4. Inspector Checklist

### AudioManager
1. Create a new empty GameObject in your MainScene.
2. Name it **AudioManager**.
3. Attach the **AudioManager.cs** script.
4. Attach an **Audio Source** component.
   - Set **Play On Awake** to `False`.
   - Set **Spatial Blend** to `0` (so it plays as flat 2D sound).

Now, assign your audio clips in the `AudioManager` component:
- [ ] `Hit Clip`
- [ ] `Enemy Defeated Clip`
- [ ] `Miss Clip`
- [ ] `Stun Clip`
- [ ] `Game Over Clip`
- [ ] `Shield Gained Clip`
- [ ] `Shield Consumed Clip`
- [ ] `Restart Click Clip`

*(If you don't have clips yet, just leave them empty. It is perfectly safe.)*

---

## 5. How to Test (With Clips)
1. Assign basic `.wav` or `.mp3` files to the slots in the inspector.
2. Press Play.
3. Attack an enemy to hear the **Hit** sound.
4. Destroy an enemy to hear the **Enemy Defeated** sound.
5. Intentionally miss an attack (without a shield) to hear the **Miss** and **Stun** sounds.
6. Build a combo up to your threshold to hear the **Shield Gained** sound.
7. Miss an attack while shielded to hear the **Shield Consumed** sound (notice the Stun sound does not play).
8. Let an enemy touch you to hear the **Game Over** sound.
9. Click the Restart button (note: the click sound might cut off quickly as the scene reloads immediately).

## 6. How to Test (Without Clips)
1. Leave all clip slots empty in the Inspector.
2. Optionally check `Log Missing Clips` to true.
3. Play the game exactly as above.
4. Confirm no game-breaking errors occur and that combat still functions normally. (If logging is enabled, you'll see helpful yellow warnings instead).

---

## 7. Regression Test

| Feature | Expected Result |
|---------|----------------|
| **Combat Logic** | Normal, Heavy, Switch, and Pattern enemies still function and die correctly. |
| **Stun / Shield Mechanics** | Misses still cause stun (or consume shield) exactly as in Phase 15. |
| **Combo / Score** | UI still updates, combo still expires on timeout. |
| **Spawning** | Enemies continue spawning with difficulty scaling and weights applied. |

---

## 8. Known Limitations

- **No Background Music:** BGM requires a looping source and is not handled by this simple SFX manager yet.
- **No Advanced Audio Routing:** All sounds play through a single AudioSource. There is no AudioMixer or volume sliders (other than the global float setting on the script).
- **Restart Cutoff:** Because the game reloads the scene immediately upon clicking Restart, the click sound may be cut off before it finishes playing.

---

## 9. Next Phase Suggestion

**Phase 17:**
- Add **Visual Polish & Juiciness**.
- Implement a floating text pop-up for the Combo counter on every hit.
- Flash the Shield UI text when a shield is gained or lost.
- Add some basic scene environment/background elements to anchor the action visually.
