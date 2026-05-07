# Phase 22 – Pause Menu Setup Guide

## 1. What Was Added in Phase 22
This phase introduces a robust pause system into the core gameplay loop.
- **PauseMenuManager**: A new script handling Time.timeScale, UI visibility, and scene navigation safely.
- **ESC Key Support**: Allows players to pause and unpause the game seamlessly.
- **Game Over Protection**: Prevents the pause menu from incorrectly opening after the player has lost.
- **Navigation Flow**: Players can now resume, restart, return to the Main Menu, or quit entirely from within a session.

---

## 2. Files Created / Changed
- `Assets/Scripts/UI/PauseMenuManager.cs` (Created)

---

## 3. Manual Setup Checklist
To hook up the new Pause Menu in your gameplay scene, perform the following steps in the Unity Editor:

### Create the Pause Panel UI
- [ ] Open your gameplay scene.
- [ ] Inside your main Canvas, create a new Panel named `PausePanel`.
- [ ] Set `PausePanel`'s background color to a semi-transparent dark shade (e.g., Black, Alpha ~150).
- [ ] Make the `PausePanel` **inactive** by default in the Inspector.
- [ ] Inside `PausePanel`, create:
  - A Title Text object (e.g., "Paused").
  - A UI Button named `ResumeButton` with text "Resume".
  - A UI Button named `RestartButton` with text "Restart".
  - A UI Button named `MainMenuButton` with text "Main Menu".
  - A UI Button named `QuitButton` with text "Quit".

### Attach the PauseMenuManager
- [ ] Create an Empty GameObject in your gameplay scene named `PauseMenuManager`.
- [ ] Attach the `PauseMenuManager.cs` script to it.
- [ ] Drag your new `PausePanel` from the Canvas into the **Pause Panel** slot on the script.
- [ ] Ensure the **Main Menu Scene Name** exactly matches your Main Menu scene name (e.g., `MainMenu`).

### Hook Up the Buttons
Select each button inside your `PausePanel` and add an **OnClick()** event targeting the `PauseMenuManager` object:
- [ ] `ResumeButton` -> `PauseMenuManager.ResumeGame`
- [ ] `RestartButton` -> `PauseMenuManager.RestartGame`
- [ ] `MainMenuButton` -> `PauseMenuManager.BackToMainMenu`
- [ ] `QuitButton` -> `PauseMenuManager.QuitGame`

### Verify Build Profiles
- [ ] Go to **File -> Build Profiles**.
- [ ] Confirm your Main Menu scene is listed at index `0`.
- [ ] Confirm your Gameplay scene is listed at index `1`.
*(Without both scenes present, the 'Back to Main Menu' button will fail).*

---

## 4. How to Test in Unity Editor
1. Open your gameplay scene and press **Play**.
2. Press `ESC`. The game should pause (`Time.timeScale = 0`) and your `PausePanel` should appear.
3. Press `ESC` again or click **Resume**. The game should seamlessly continue.
4. Press `ESC` and click **Restart**. The gameplay scene should reload fresh.
5. Press `ESC` and click **Main Menu**. You should transition to the MainMenu scene.
6. Press `ESC` and click **Quit**. You should see a log message: *"Quit requested. Application.Quit() does not close the Editor play mode."*
7. Finally, let an enemy hit you to trigger **Game Over**. Press `ESC`. Confirm the pause menu *does not* appear over the Game Over screen.

---

## 5. How to Test in Standalone Build
1. Build the game to your `Builds/Windows/` directory.
2. Launch the game; the Main Menu will load first.
3. Click **Play** to enter gameplay.
4. Press `ESC` to pause. Confirm enemy movement and spawning stops.
5. Verify the **Resume**, **Restart**, and **Main Menu** buttons function exactly as they did in the Editor.
6. Verify clicking **Quit** closes the application window.

---

## 6. Known Limitations
- The Pause Panel UI is currently a functional placeholder without cohesive styling.
- There is no Settings Menu to adjust volume or keybindings.
- The pause transition is instantaneous; there are no fade or slide animations yet.

---

## 7. Next Phase Suggestion
**Phase 23: UI Polish / Menu Styling**
Now that the core navigational flow (Main Menu -> Gameplay -> Pause -> Main Menu) is fully established, the next step is to replace the functional placeholder UI with stylized, cohesive menus, buttons, and animations. Alternatively, we can begin asset replacement for the Player visual if the art is ready.
