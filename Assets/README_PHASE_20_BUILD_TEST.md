# Phase 20 – Build Preparation & Testing

## 1. Phase 20 Objective
The goal of this phase is to ensure the prototype can be successfully exported as a standalone playable build. It verifies that no editor-only code breaks the build process and that the core mechanics, UI, and scene reload logic function correctly outside of the Unity Editor. **No new gameplay features have been added.**

---

## 2. Build Preparation Checklist
Before creating a build, verify the following:
- [ ] **Scene Settings**: The gameplay scene is added to **Build Profiles** (or Build Settings) and is checked (enabled). *(If it's named `SampleScene`, it's safe to rename it to `GameplayScene` later).*
- [ ] **TextMeshPro**: TMP Essentials are imported so UI text renders correctly.
- [ ] **Audio Safety**: `AudioManager` exists in the scene, and the Main Camera has an `AudioListener`. (Missing clips will not break the game, but the AudioSource/Listener components must exist).
- [ ] **Event System**: An `EventSystem` object exists in the scene (created automatically with Canvas) so the Restart button works.
- [ ] **Manager References**: `GameUI`, `EnemySpawner`, `DifficultyManager`, and `PlayerCombat`/`PlayerMovement` all have their required inspector references assigned.
- [ ] **No Errors**: The Unity Console shows zero red errors before attempting to build.

---

## 3. Unity 6.3 Build Instructions
To create a standalone build:
1. Go to **File → Build Profiles**.
2. Select your target platform (e.g., **Windows**, **macOS**, or **WebGL**).
3. Ensure your gameplay scene is present in the **Scene List**. Since we do not have a Main Menu yet, this must be the first (and only) scene in the list.
4. Click **Build**.
5. When prompted to select a folder, **do not save the build inside the `Assets` folder**. 
6. Instead, create a new folder at the project root named `Builds` (e.g., `DAKH/Builds/Windows/`).
7. Once the build completes, open the folder and run the generated executable (e.g., `.exe`).

---

## 4. Git Ignore Recommendation
Build outputs should not be committed to version control because they are large binary files that change constantly.
The project's `.gitignore` already correctly ignores:
- `[Bb]uild/`
- `[Bb]uilds/`
As long as you export to the recommended `Builds/` folder, your generated files will be safely ignored by Git.

---

## 5. Standalone Build Testing Checklist
Once the game is running as a standalone executable, manually verify the following:
- [ ] **Startup**: The game launches without crashing and goes straight into gameplay.
- [ ] **Spawning**: Enemies spawn correctly from both sides.
- [ ] **Combat**: Player can attack left and right, hitting enemies.
- [ ] **UI Scaling**: The Score, Combo, and Shield text elements are readable and do not scale wildly off-screen. *(If they do, change Canvas Scaler to "Scale With Screen Size").*
- [ ] **Miss / Stun**: Missing an attack correctly triggers the stun penalty.
- [ ] **Enemy Types**: Normal, Heavy, Switch, and Pattern3Hit enemies all function as expected.
- [ ] **Weighted Spawning**: Different enemy types spawn based on difficulty progression.
- [ ] **Game Over**: Letting an enemy touch the player correctly stops the game and shows the Game Over panel.
- [ ] **Restart**: Clicking the Restart button correctly reloads the scene and resets the session cleanly.
- [ ] **Audio**: If clips were assigned, hit/miss/UI sounds play as expected.

---

## 6. Known Limitations
- **No Main Menu**: The game boots directly into the action.
- **No Pause Menu**: You cannot pause the game or exit cleanly (must use Alt+F4 or window controls).
- **Placeholder Aesthetics**: The UI, character art, and backgrounds are still placeholders.
- **Resolution Support**: UI has not been extensively tested across ultrawide or obscure aspect ratios.

---

## 7. Next Phase Suggestion
- **Phase 21: Main Menu Scene**. Create a dedicated Main Menu scene with "Play" and "Quit" buttons. Add this scene to index `0` in your Build Profiles so the game boots there first. You can also implement a simple in-game Pause Menu.
