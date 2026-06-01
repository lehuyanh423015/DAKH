# Phase 26 - UI And Background Polish

## What Was Added

- `UIPanelStyler`: applies a reusable semi-transparent panel background.
- `UIButtonStyler`: applies consistent button and TextMeshPro styling.
- `MenuUIAutoLayout`: quickly aligns `MainPanel` and `SettingsPanel`.
- `BackgroundVideoPlayer`: prepares optional looped background video support.
- UI polish workflow for menu, settings, pause, and game-over panels.
- Future video background support without requiring a video asset now.

## Files Created/Changed

- `Assets/Scripts/UI/UIPanelStyler.cs`
- `Assets/Scripts/UI/UIButtonStyler.cs`
- `Assets/Scripts/UI/MenuUIAutoLayout.cs`
- `Assets/Scripts/Environment/BackgroundVideoPlayer.cs`
- `Assets/README_PHASE_26_UI_AND_BACKGROUND_POLISH.md`

No gameplay logic was changed.

## MainMenu Setup Checklist

- [ ] `MainPanel` has `UIPanelStyler`.
- [ ] `SettingsPanel` has `UIPanelStyler`.
- [ ] `PlayButton` has `UIButtonStyler`.
- [ ] `SettingsButton` has `UIButtonStyler`.
- [ ] `QuitButton` has `UIButtonStyler`.
- [ ] `BackButton` has `UIButtonStyler`.
- [ ] `MenuUIAutoLayout` is added to the Canvas or a MainMenu UI manager object.
- [ ] `MenuUIAutoLayout.mainPanel` is assigned to `MainPanel`.
- [ ] `MenuUIAutoLayout.settingsPanel` is assigned to `SettingsPanel`.
- [ ] MainMenu demo remains visible behind the panel.
- [ ] Menu buttons remain clickable.

Expected hierarchy:

```text
Canvas
|-- MainPanel
|   |-- TitleText
|   |-- SubtitleText optional
|   |-- PlayButton
|   |-- SettingsButton
|   `-- QuitButton
`-- SettingsPanel
    |-- SettingsTitleText
    |-- FullscreenToggle
    |-- ResolutionDropdown
    |-- VolumeLabelText
    |-- MasterVolumeSlider
    `-- BackButton
```

Keep existing `MainMenuManager`, `SettingsManager`, and `MenuPanelSwitcher` components.

## Pause/GameOver Setup Checklist

- [ ] `PausePanel` has `UIPanelStyler`.
- [ ] `GameOverPanel` has `UIPanelStyler`.
- [ ] Pause menu buttons have `UIButtonStyler`.
- [ ] GameOver `RestartButton` has `UIButtonStyler`.
- [ ] Main Menu and Quit buttons, if present, have `UIButtonStyler`.

This phase does not force scene changes through code. Add these components in the Unity Inspector.

## Background Video Setup Checklist For Later

- [ ] Create a `RawImage` named `BackgroundVideoRawImage` under the Canvas.
- [ ] Put `BackgroundVideoRawImage` behind menu panels in the hierarchy.
- [ ] Stretch `BackgroundVideoRawImage` full screen.
- [ ] Create a RenderTexture asset, for example `Assets/Art/Background/BackgroundLoop.renderTexture`.
- [ ] Add `BackgroundVideoPlayer` to `BackgroundVideoRawImage` or a `BackgroundManager` object.
- [ ] Assign the RawImage to `targetRawImage`.
- [ ] Assign the RenderTexture to `renderTexture`.
- [ ] Assign a VideoClip when available.
- [ ] Enable loop.
- [ ] If no video exists yet, leave `videoClip` empty. The RawImage will hide safely.

Recommended hierarchy:

```text
Canvas
|-- BackgroundVideoRawImage
|-- MainPanel
`-- SettingsPanel
```

## How To Test Now

1. Open `MainMenu`.
2. Add `UIPanelStyler` to `MainPanel` and `SettingsPanel`.
3. Add `UIButtonStyler` to menu buttons.
4. Add and assign `MenuUIAutoLayout`.
5. Press Play.
6. Confirm `MainPanel` is readable over the demo.
7. Confirm buttons are visible and clickable.
8. Click Settings.
9. Confirm `SettingsPanel` is readable.
10. Click Back.
11. Click Play.
12. Confirm the gameplay scene loads normally.
13. In gameplay, confirm Pause and GameOver panels remain functional after adding their style components.

## How To Add Background Video Later

1. Import the video file into `Assets/Art/Background`.
2. Create a RenderTexture asset.
3. Create or enable `BackgroundVideoRawImage`.
4. Add or select `BackgroundVideoPlayer`.
5. Assign the VideoClip.
6. Assign the RenderTexture.
7. Assign the target RawImage.
8. Press Play and confirm the video loops.
9. If the video has audio, keep `muteAudio` enabled unless background audio is desired.

## Recommended Video Format Notes

- Use a short seamless loop.
- Keep file size reasonable.
- Prefer 1920x1080 or 1280x720.
- Avoid too much brightness or contrast behind UI.
- Keep the menu panel overlay readable over the video.

## Known Limitations

- No custom font is included.
- No advanced button animations are included.
- No background video asset is included.
- UI style is functional polish, not final art.
- Video may need build testing later.
