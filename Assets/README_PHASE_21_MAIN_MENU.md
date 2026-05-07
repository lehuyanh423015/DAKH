# Phase 21 – Main Menu

## 1. What Was Added in Phase 21
This phase establishes the entry point for the game by introducing a dedicated Main Menu.
- **MainMenu Scene**: A new scene to serve as the application's starting point.
- **MainMenuManager**: A script to handle button clicks for starting the game and quitting the application.
- **Build Profiles Updates**: Instructions to ensure the game launches into the Main Menu first instead of dropping the player immediately into gameplay.

---

## 2. Files Created / Changed
- `Assets/Scripts/UI/MainMenuManager.cs` (Created)
- `Assets/Scenes/MainMenu.unity` (To be created manually in Unity)

---

## 3. Manual Setup Checklist
To fully implement the Main Menu in your project, perform the following steps in the Unity Editor:

- [ ] **Create the Scene**: Go to `Assets/Scenes/` and create a new Scene named `MainMenu`. Open it.
- [ ] **Canvas Setup**: Add a **UI -> Canvas** to the scene. Set the Render Mode to *Screen Space - Overlay*.
- [ ] **EventSystem**: Ensure an **EventSystem** exists in the scene (Unity usually creates this automatically with the Canvas).
- [ ] **UI Elements**: Inside the Canvas, create:
  - A TextMeshPro Text object for the Title (e.g., "DAKH").
  - A UI Button (TextMeshPro) named `PlayButton` with text "Play".
  - A UI Button (TextMeshPro) named `QuitButton` with text "Quit".
- [ ] **Manager Setup**: Create an Empty GameObject named `MainMenuManager`. Attach the `MainMenuManager.cs` script to it.
- [ ] **Configure Manager**: In the `MainMenuManager` inspector, ensure the `Gameplay Scene Name` exactly matches your gameplay scene (e.g., `SampleScene` or `GameplayScene`).
- [ ] **Hook Up Buttons**:
  - Select the `PlayButton`. In the OnClick() list, add a new entry. Drag the `MainMenuManager` object into the object slot, and select `MainMenuManager -> PlayGame`.
  - Select the `QuitButton`. In the OnClick() list, add a new entry. Drag the `MainMenuManager` object into the object slot, and select `MainMenuManager -> QuitGame`.

---

## 4. Unity 6.3 Build Profiles Setup
To ensure the build launches into the Main Menu:
1. Open **File → Build Profiles**.
2. If `SampleScene` (or your gameplay scene) is already in the Scene List, leave it there.
3. Open the `MainMenu` scene in the editor, and click **Add Open Scenes** in the Build Profiles window.
4. **Drag `MainMenu` to the top of the list** so it has index `0`.
5. Ensure your gameplay scene is right below it, with index `1`.
6. Both scenes must have their checkboxes checked.

---

## 5. How to Test in Unity Editor
1. Open the `MainMenu` scene and press **Play**.
2. Click the **Play** button. You should seamlessly transition into the gameplay scene.
3. Stop Play Mode and return to the `MainMenu` scene.
4. Press **Play** again.
5. Click the **Quit** button. You should see a message in the Unity Console: *"Quit requested. Application.Quit() does not close the Editor play mode."*

---

## 6. How to Test in Standalone Build
1. Build the game via Build Profiles to your `Builds/Windows/` folder.
2. Launch the exported `.exe`.
3. Confirm the game boots directly into the Main Menu.
4. Click **Play** and verify the game starts normally.
5. Verify that during gameplay, losing and clicking **Restart** still works correctly (it should reload the gameplay scene, not the Main Menu).
6. Relaunch the `.exe` to return to the Main Menu.
7. Click **Quit** and confirm the application closes entirely.

---

## 7. Known Limitations
- The Main Menu UI is completely placeholder and unstyled.
- There is currently no Settings Menu.
- There is no Pause Menu to return to the Main Menu from gameplay.
- No background music or animations are present in the menu.

---

## 8. Next Phase Suggestion
**Phase 22: Pause Menu**. 
Now that we have a Main Menu, players need a way to pause the game and return to it. We should introduce a Pause Panel in the gameplay scene triggered by `ESC`, with options to Resume, Restart, or return to the Main Menu.
