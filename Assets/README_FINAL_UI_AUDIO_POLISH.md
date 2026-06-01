# Final UI Audio Polish

## What Was Added

- GameOver music support in `SceneMusicController`.
- Final MainMenu left-side layout with `FinalMenuLayoutApplier`.
- Highlight-only MainMenu style so the AI demo remains visible.
- Consistent cyber/night button style in `UIButtonStyler`.
- Reusable text styling through `UITextStyler`.
- Shared Settings/Pause/GameOver panel styling workflow.

## Files Changed

- `Assets/Scripts/Audio/SceneMusicController.cs`
- `Assets/Scripts/UI/UIButtonStyler.cs`
- `Assets/Scripts/UI/UITextStyler.cs`
- `Assets/Scripts/UI/FinalMenuLayoutApplier.cs`
- `Assets/README_FINAL_UI_AUDIO_POLISH.md`

## Manual Setup Checklist

### Audio

- [ ] `GameplayMusic` has the gameplay music clip assigned to `Music Clip`.
- [ ] `GameplayMusic` has `Game Over Music Clip` assigned.
- [ ] `Play Game Over Music On Game Over = true`.
- [ ] `Stop Gameplay Music On Game Over = true`.
- [ ] `Game Over Music Loops` is set as desired.
- [ ] `MainMenuMusic` still has the menu music clip assigned.
- [ ] `MainMenuMusic` does not use `DontDestroyOnLoad`.
- [ ] `GameplayMusic` does not use `DontDestroyOnLoad`.

### MainMenu

- [ ] `MainPanel` is left-side.
- [ ] `MainPanel` background Image is disabled or transparent.
- [ ] `FinalMenuLayoutApplier` is added to Canvas or a menu UI manager.
- [ ] `FinalMenuLayoutApplier.mainPanel` is assigned.
- [ ] `FinalMenuLayoutApplier.settingsPanel` is assigned.
- [ ] Title uses `UITextStyler`.
- [ ] Subtitle uses `UITextStyler`.
- [ ] Play/Settings/Quit buttons use `UIButtonStyler`.
- [ ] AI demo remains visible behind the menu.

Recommended text:

- Title: `DAKH`
- Subtitle: `LEFT - RIGHT - SURVIVE`
- Buttons: `PLAY`, `SETTINGS`, `QUIT`

### Settings/Pause/GameOver

- [ ] Panels have subtle readable backgrounds.
- [ ] SettingsPanel can keep `UIPanelStyler`.
- [ ] PausePanel can keep `UIPanelStyler`.
- [ ] GameOverPanel can keep `UIPanelStyler`.
- [ ] Buttons use `UIButtonStyler`.
- [ ] Titles use `UITextStyler`.
- [ ] Existing OnClick events still work.

Recommended title text:

- Settings: `SETTINGS`
- Pause: `PAUSED`
- GameOver: `GAME OVER`

## How To Test Audio

1. Enter Gameplay.
2. Confirm gameplay music starts.
3. Trigger GameOver.
4. Confirm gameplay freezes.
5. Confirm gameplay music stops or changes.
6. Confirm GameOver music plays.
7. Click Restart.
8. Confirm gameplay music restarts from the beginning.
9. Return to MainMenu.
10. Confirm menu music plays.

## How To Test UI

1. Open MainMenu.
2. Confirm menu is left-side and does not cover the AI demo too much.
3. Confirm there is no large dark MainPanel background.
4. Confirm buttons are readable over the video/background.
5. Open Settings.
6. Confirm SettingsPanel is readable.
7. Enter Gameplay.
8. Test Pause.
9. Test GameOver.
10. Confirm all buttons work.

## Known Limitations

- No separate music/SFX volume sliders yet.
- No crossfade.
- No advanced menu animation.
- UI is final polish for the current deadline, not a full art redesign.
