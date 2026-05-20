# Phase 23 – Settings Menu Setup Guide

## 1. What Was Added in Phase 23
- **SettingsManager**: Applies and persists fullscreen mode, resolution, and master volume using `PlayerPrefs`.
- **MenuPanelSwitcher**: Switches between the Main Panel and the Settings Panel in the Main Menu scene without any code coupling.
- **Persistence**: Settings survive closing and relaunching the game.
- **Safe startup defaults**: First launch starts windowed at 1280×720 with volume at 1.0.

---

## 2. Files Created / Changed
- `Assets/Scripts/UI/SettingsManager.cs` (Created)
- `Assets/Scripts/UI/MenuPanelSwitcher.cs` (Created)

---

## 3. Unity Player Settings – Windowed Default
Before testing, ensure Unity does not force fullscreen on launch:

1. Go to **Edit → Project Settings → Player → Resolution and Presentation**.
2. Set **Fullscreen Mode** to `Windowed`.
3. Set **Default Screen Width** to `1280`.
4. Set **Default Screen Height** to `720`.

This ensures the first launch matches the SettingsManager default before any PlayerPrefs have been saved.

---

## 4. Manual Setup Checklist

### Main Menu Scene – Canvas Structure
Build your Canvas to match this hierarchy:

```
Canvas
├── MainPanel
│   ├── TitleText          ("DAKH")
│   ├── PlayButton
│   ├── SettingsButton
│   └── QuitButton
└── SettingsPanel          ← inactive by default
    ├── SettingsTitleText  ("Settings")
    ├── FullscreenToggle
    ├── ResolutionDropdown (TMP_Dropdown)
    ├── MasterVolumeSlider
    └── BackButton
```

### Create the Manager Objects
- [ ] Create an Empty GameObject named `MenuPanelSwitcher`. Attach `MenuPanelSwitcher.cs`.
  - Assign `MainPanel` to the **Main Panel** slot.
  - Assign `SettingsPanel` to the **Settings Panel** slot.
- [ ] Create an Empty GameObject named `SettingsManager`. Attach `SettingsManager.cs`.
  - Assign `FullscreenToggle` to the **Fullscreen Toggle** slot.
  - Assign `ResolutionDropdown` to the **Resolution Dropdown** slot.
  - Assign `MasterVolumeSlider` to the **Master Volume Slider** slot.

### Hook Up All Buttons/Controls
| Control | Event | Target | Method |
|---------|-------|--------|--------|
| SettingsButton | OnClick | MenuPanelSwitcher | `ShowSettings` |
| BackButton | OnClick | MenuPanelSwitcher | `ShowMain` |
| FullscreenToggle | OnValueChanged | SettingsManager | `OnFullscreenChanged` |
| ResolutionDropdown | OnValueChanged | SettingsManager | `OnResolutionChanged` |
| MasterVolumeSlider | OnValueChanged | SettingsManager | `OnMasterVolumeChanged` |
| MasterVolumeSlider | OnPointerUp *(via EventTrigger)* | SettingsManager | `OnMasterVolumeReleased` |

> **Note on Volume Save**: To avoid writing to PlayerPrefs on every slider frame, `OnMasterVolumeChanged` only updates the live volume. `OnMasterVolumeReleased` saves it. To wire this up: select the Slider, add an `EventTrigger` component, add the **Pointer Up** event type, and link it to `SettingsManager.OnMasterVolumeReleased`.

> **Alternative simpler approach**: if you don't want the EventTrigger hassle, change the slider's **OnValueChanged** to call `OnMasterVolumeChanged` AND add a second entry pointing to `OnMasterVolumeReleased`. It will save slightly more often but is functionally correct.

---

## 5. How to Test in Unity Editor
1. Open the **MainMenu** scene and press **Play**.
2. Click **Settings**. The `SettingsPanel` should appear and `MainPanel` should hide.
3. Toggle **Fullscreen**. The window mode should switch.
4. Change the **Resolution** dropdown. The resolution should update.
5. Drag the **Volume** slider. Confirm audio level changes (if any AudioSources are playing in the background).
6. Click **Back**. The `MainPanel` should reappear.
7. Stop Play Mode and press Play again. Confirm the dropdown and toggle reflect your saved values (they are stored in PlayerPrefs).
8. Click **Play** and confirm the gameplay scene loads and functions normally.

---

## 6. How to Test in Standalone Build
1. Build the game to your `Builds/Windows/` folder.
2. Launch the `.exe`. Confirm it opens **windowed at 1280×720** (if Player Settings were set correctly).
3. Click **Settings**.
4. Toggle **Fullscreen** → game should switch to fullscreen.
5. Change resolution → window should resize.
6. Adjust volume → audio output level should change.
7. Close the game.
8. Relaunch the `.exe`. Confirm settings persisted (fullscreen toggle state, resolution, volume are restored).

---

## 7. Known Limitations
- No separate Music / SFX volume sliders yet (only master volume via `AudioListener.volume`).
- No AudioMixer groups are configured.
- No key remapping.
- Resolution changes in the Editor may behave differently than in a standalone build.
- Settings panel UI is placeholder-styled.

---

## 8. Next Phase Suggestion
**Phase 24: Player Asset Replacement**
With all menus and settings in place, the project is ready for visual polish. The next step is to replace the placeholder square for the Player with real artwork, following the Phase 19 Visual Architecture guide (root keeps scripts/colliders, `Visual` child holds the `SpriteRenderer` and future `Animator`).
