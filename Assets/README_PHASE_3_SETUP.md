# Phase 3 – UI & Restart Flow Setup Guide

## 1. What Was Added in Phase 3

| Feature | Details |
|---------|---------|
| **Score UI** | Live score displayed on screen via `scoreText`. |
| **Combo UI** | Live combo displayed on screen via `comboText`. Both reset correctly on miss. |
| **Game Over Panel** | Hidden during play; appears automatically when an enemy reaches the player. |
| **Final Score display** | Game Over panel shows the exact score at the moment of game over. |
| **Restart Button** | Clicking it calls `SceneManager.LoadScene(...)`, resetting everything. |

---

## 2. Files Created / Changed

### `GameManager.cs` — **updated**
Added two C# events:
```csharp
public event System.Action<int, int> OnScoreComboChanged;
public event System.Action<int, int> OnGameOverEvent;
```
- `OnScoreComboChanged` is invoked at the end of `RegisterHit()` and `RegisterMiss()`.
- `OnGameOverEvent` is invoked at the end of `GameOver()`.
- All Phase 1 and Phase 2 Console logs are preserved.

### `GameUI.cs` — **created** (`Assets/Scripts/UI/GameUI.cs`)
New script that:
- Subscribes to GameManager events in `Start()`.
- Unsubscribes in `OnDisable()` to avoid memory leaks.
- Updates `scoreText` / `comboText` on every `OnScoreComboChanged` call.
- Shows `gameOverPanel` and sets `finalScoreText` on `OnGameOverEvent`.
- Hooks the `restartButton` click listener to `RestartGame()`.

> **TextMeshPro note**: `GameUI.cs` uses `TextMeshProUGUI`.
> If TMP is not yet imported, go to **Window → TextMeshPro → Import TMP Essential Resources**.
> Alternatively, replace `TextMeshProUGUI` with `UnityEngine.UI.Text` and remove `using TMPro;`.

---

## 3. Required Scene Hierarchy

Build this exact hierarchy in the Unity Hierarchy window:

```
Canvas                          ← UI → Canvas (Screen Space – Overlay)
├── ScoreText                   ← UI → Text - TextMeshPro
├── ComboText                   ← UI → Text - TextMeshPro
└── GameOverPanel               ← UI → Panel (or empty with Image)
    ├── GameOverTitleText        ← UI → Text - TextMeshPro   ("GAME OVER")
    ├── FinalScoreText           ← UI → Text - TextMeshPro   ("Final Score: 0")
    └── RestartButton            ← UI → Button - TextMeshPro

GameUI                          ← empty GameObject (or attach to Canvas itself)
```

---

## 4. Inspector Checklist

### Canvas
- [ ] A `Canvas` exists in the scene (add via **GameObject → UI → Canvas**).
- [ ] Canvas `Render Mode` is **Screen Space – Overlay** (default).
- [ ] An `EventSystem` exists (added automatically with the Canvas).

### HUD texts
- [ ] `ScoreText` (TextMeshPro) exists as a child of Canvas.
- [ ] `ComboText` (TextMeshPro) exists as a child of Canvas.
- [ ] Recommended positions:
  - `ScoreText`: Anchor top-left, Pos `(120, -30, 0)`.
  - `ComboText`: Anchor top-left, Pos `(120, -70, 0)`.

### Game Over Panel
- [ ] `GameOverPanel` exists as a child of Canvas.
- [ ] `GameOverPanel` is **centred** (Anchor = middle-center, Pos = `0, 0, 0`).
- [ ] `GameOverPanel` contains `GameOverTitleText`, `FinalScoreText`, and `RestartButton`.
- [ ] `GameOverPanel` is **inactive** at the start (uncheck the checkbox next to its name in the Inspector, or the script hides it in `Start()`).

### GameUI script
- [ ] A `GameUI` empty GameObject exists (or `GameUI.cs` is attached to the Canvas).
- [ ] `GameUI.cs` is attached to that object.
- [ ] **`scoreText`** → drag `ScoreText` from the Hierarchy.
- [ ] **`comboText`** → drag `ComboText` from the Hierarchy.
- [ ] **`gameOverPanel`** → drag `GameOverPanel` from the Hierarchy.
- [ ] **`finalScoreText`** → drag `FinalScoreText` (child of `GameOverPanel`).
- [ ] **`restartButton`** → drag `RestartButton` (child of `GameOverPanel`).

---

## 5. How to Test Score UI

1. Press **Play**.
2. Wait for an enemy to enter attack range.
3. Press the correct direction key.
4. ✅ `ScoreText` on screen updates immediately (e.g. `Score: 110`).

---

## 6. How to Test Combo UI

1. Hit multiple enemies consecutively without missing.
2. ✅ `ComboText` increments: `Combo: 1` → `Combo: 2` → `Combo: 3` …
3. Miss an attack (press key with no enemy in range).
4. ✅ `ComboText` resets: `Combo: 0`.

---

## 7. How to Test Game Over UI

1. Let an enemy walk all the way into the player.
2. ✅ `GameOverPanel` appears in the centre of the screen.
3. ✅ `FinalScoreText` shows `Final Score: <your score>`.
4. ✅ Console still prints `Game Over`, `Final Score: ...`, `Final Combo: ...` (Phase 2 logs preserved).

---

## 8. How to Test Restart

1. After the Game Over panel appears, click the **Restart** button.
2. ✅ The scene reloads from the beginning.
3. ✅ `ScoreText` resets to `Score: 0`.
4. ✅ `ComboText` resets to `Combo: 0`.
5. ✅ Enemies start spawning again.
6. ✅ `GameOverPanel` is hidden.

> **Important**: Make sure the scene is added to **File → Build Settings → Scenes In Build**.
> `SceneManager.LoadScene` requires this to work in the Editor and in builds.

---

## 9. Known Limitations

- ❌ UI is basic (no styling, no animations).
- ❌ No transition animation on the Game Over panel.
- ❌ No sound effects yet.
- ❌ No hit flash / visual effects yet.
- ❌ No main menu yet.
- ❌ No difficulty scaling yet.
- ❌ No multiple enemy types yet.
- ⚠️ The Game Over panel is hidden by the script in `Start()`.
  If you also want it hidden in Edit mode, deactivate it manually in the Hierarchy
  (uncheck the checkbox next to `GameOverPanel`'s name).

---

## 10. Next Phase – Phase 4 Suggestions

| Feature | Implementation hint |
|---------|---------------------|
| **Hit flash** | On enemy destroy, briefly set `SpriteRenderer.color = Color.red` for one frame via coroutine. |
| **Attack indicator** | Show a directional arrow or highlight when a key is pressed. |
| **Stun visual** | Briefly tint the player sprite while `isStunned` is true. |
| **Simple SFX** | `AudioSource.PlayOneShot()` for hit, miss, and game over. |
| **Difficulty scaling** | Gradually decrease `spawnInterval` and increase `moveSpeed` over time. |
| **Game feel tweaks** | Screen shake on miss, enemy death particle via `ParticleSystem.Play()`. |
