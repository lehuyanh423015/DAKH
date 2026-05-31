# Phase 25: Full Gameplay Attract Mode (Main Menu Demo)

## 1. What Was Added
- **Main Menu Gameplay Demo**: The Main Menu can now feature a full-screen, fully functional gameplay simulation running in the background.
- **Game Mode States**: `GameManager.cs` now supports `Normal` and `Demo` modes.
- **Demo AI**: `DemoPlayerAI.cs` autonomously controls the player in demo mode, seeking out and attacking enemies accurately. It interfaces cleanly with `PlayerCombat`'s new public attack requests.
- **Demo-Safe Enemy Contact**: If an enemy touches the player during `Demo` mode, it is harmlessly destroyed rather than triggering a Game Over.
- **Demo Difficulty Tuning**: Fixed overriding logic in `DifficultyManager.cs` keeps the demo paced evenly, while `EnemySpawner.cs` explicitly cycles through all enemy variants to demonstrate the full game loop.

---

## 2. Files Changed

| File | Changes Made |
|------|--------------|
| `Assets/Scripts/Core/GameManager.cs` | Added `GameMode` enum and toggle. If in `Demo` mode, `GameOver()` simply logs and resets the combo without freezing the game or showing the Game Over panel. |
| `Assets/Scripts/Enemies/Enemy.cs` | Intercepts `OnTriggerEnter2D`. If `IsDemoMode` is true, the enemy is safely made harmless and destroyed instead of killing the player. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Added `allowHumanInput` flag to disable keyboard input. Opened up `RequestAttackLeft()`, `RequestAttackRight()`, and `RequestAttack(int)` so an AI can drive the player's attacks using the exact same logic. |
| `Assets/Scripts/Demo/DemoPlayerAI.cs` | **NEW SCRIPT**. Finds the closest enemy within range and triggers the corresponding attack command on `PlayerCombat`. |
| `Assets/Scripts/Core/DifficultyManager.cs` | Added optional `Demo` mode overrides that lock spawn intervals and enemy speeds to fixed, readable values. |
| `Assets/Scripts/Spawning/EnemySpawner.cs` | Added `cycleEnemyTypesInDemo` flag. When enabled in Demo mode, the spawner perfectly cycles through the assigned `enemyPrefabs` sequentially instead of relying on weighted randomness. |
| `Assets/Scripts/UI/MainMenuManager.cs` | Added a `demoGameplayRoot` reference. Clicking "Play" disables the entire demo root to prevent lingering audio/scripts before safely loading the real GameplayScene. |

---

## 3. Important Design Explanations

The demo is a completely authentic slice of the game, rather than a fake pre-animated UI loop. 
- It uses the real `PlayerCombat`, `Enemy`, and `DifficultyManager` systems. 
- The AI plays strictly by the rules—it doesn't arbitrarily kill enemies; it pushes the exact same internal "buttons" a human player would.
- By intercepting Game Over states natively within `GameManager` and `Enemy`, we completely eliminate the risk of a demo session accidentally triggering a real game over screen, saving progress, or freezing the menu.

---

## 4. Main Menu Setup Checklist

To build your full-screen demo in the MainMenu scene, do the following:

1. Duplicate the core objects from your normal `GameplayScene` (Player, GameManager, Spawners, Cameras) and paste them into the MainMenu scene under a single empty GameObject named **`DemoGameplayRoot`**.
2. **GameManager**: Set `Game Mode` to `Demo`.
3. **PlayerCombat**: Uncheck `Allow Human Input`.
4. **DemoPlayerAI**: Attach this to your Player object. Assign the `PlayerCombat` and `PlayerTransform` references.
5. **DifficultyManager**: Check `Use Demo Difficulty Override` and adjust the speeds as necessary.
6. **EnemySpawner**: Ensure all 4 enemy prefabs are assigned. Check `Cycle Enemy Types In Demo`.
7. **MainMenuManager**: Assign the `DemoGameplayRoot` object into the corresponding slot on this script.
8. **Clean up**: Ensure the `PauseMenuManager` and `GameUI` (GameOver panels) are either deleted or disabled in the MainMenu scene, as they are not needed.

---

## 5. GameplayScene Checklist

Ensure your primary gameplay scene remains pristine:

- [ ] `GameManager` Game Mode is set to **Normal**.
- [ ] `PlayerCombat` **Allow Human Input** is checked.
- [ ] `DemoPlayerAI` is **NOT** present or enabled on the real player.

---

## 6. How to Test

### MainMenu Demo Test
1. Load the `MainMenu` scene and press Play in the Unity Editor.
2. Confirm that enemies begin spawning and walking toward the centre.
3. Confirm that the AI player automatically attacks them accurately on both sides.
4. Watch for all enemy types (Normal, Heavy, Switch, Pattern3Hit) to appear sequentially.
5. If an enemy slips past the AI and touches it, confirm that it vanishes gracefully without popping a Game Over screen.
6. Click the "Play" button on the UI. The real GameplayScene should load instantly.

### Real Gameplay Test
1. Start directly in the `GameplayScene`.
2. Confirm your A / D keys still control the player normally.
3. Confirm that missing or getting hit triggers a standard Game Over freeze, and that the AI does not attempt to play for you.

---

## 7. Known Limitations

- The AI is highly reactive and purely distance-based. While it perfectly handles Normal and Heavy enemies, it might look slightly robotic against the SwitchEnemy.
- As the demo uses fixed difficulty variables, it won't showcase the game's high-speed endgame intensity. (This is intentional, as it keeps the menu readable).
