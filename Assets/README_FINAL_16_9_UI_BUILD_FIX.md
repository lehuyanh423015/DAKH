# Final 16:9 UI Build Fix

## Target Aspect Ratio

The game targets 16:9 only for the final build.

Recommended supported resolutions:

- 1280x720
- 1920x1080

`SettingsManager` now exposes only these 16:9 resolutions.

## Recommended Project Settings

In Unity Player Settings:

- Fullscreen Mode = Windowed
- Default Width = 1280
- Default Height = 720
- Resizable Window = false for safest final build

This reduces layout differences between the Unity Editor Game view and the standalone build.

## Canvas Scaler Setup

For both MainMenu Canvas and Gameplay Canvas:

- Add `CanvasResolutionSetup`
- Keep `Apply On Start = true`
- Reference Resolution = 1920x1080
- Screen Match Mode = Match Width Or Height
- Match = 0.5

The script applies:

- `CanvasScaler.uiScaleMode = Scale With Screen Size`
- `CanvasScaler.referenceResolution = 1920x1080`
- `CanvasScaler.screenMatchMode = Match Width Or Height`
- `CanvasScaler.matchWidthOrHeight = 0.5`

## MainPanel Background

The MainMenu `MainPanel` background Image is intentionally disabled.

Reason:

- MainMenu should show highlighted text/buttons only.
- The AI gameplay demo/video should remain clearly visible.
- No large dark or blurred panel should appear behind the menu.

`FinalMenuLayoutApplier` disables the `Image` component directly on `MainPanel` only. It does not disable Images on child buttons.

## UIPanelStyler Safety

`UIPanelStyler` now has:

- `Add Image If Missing`

For `MainPanel`, preferred setup:

- Remove `UIPanelStyler`, or
- Disable `UIPanelStyler`, or
- Set `Add Image If Missing = false`

Settings/Pause/GameOver panels can keep their background Images and `UIPanelStyler`.

## Manual Setup Checklist

### MainMenu

- [ ] MainMenu Canvas has `CanvasResolutionSetup`.
- [ ] MainMenu Canvas has `CanvasScaler`.
- [ ] `FinalMenuLayoutApplier` is assigned to `MainPanel`.
- [ ] `FinalMenuLayoutApplier.Disable Main Panel Background Image = true`.
- [ ] `MainPanel` has no active background Image.
- [ ] Button Images are still enabled.
- [ ] AI demo/video remains visible.

### Gameplay

- [ ] Gameplay Canvas has `CanvasResolutionSetup`.
- [ ] Gameplay Canvas has `CanvasScaler`.
- [ ] PausePanel can keep a readable background.
- [ ] GameOverPanel can keep a readable background.

### Build

- [ ] Player Settings default resolution is 1280x720.
- [ ] Fullscreen mode is Windowed.
- [ ] Resizable Window is false.
- [ ] Settings menu shows only 1280x720 and 1920x1080.

## How To Test In Editor

1. Open the Game view.
2. Test 1280x720.
3. Test 1920x1080.
4. Open MainMenu.
5. Confirm MainPanel stays left-side.
6. Confirm no large dark/blurred MainPanel background appears after pressing Play.
7. Confirm buttons/text remain readable.
8. Enter Gameplay.
9. Test Pause and GameOver panels.

## How To Test Standalone Build

1. Build the game.
2. Launch at 1280x720 windowed.
3. Confirm MainMenu matches the Editor layout.
4. Open Settings and switch to 1920x1080.
5. Confirm UI still scales correctly.
6. Return to gameplay.
7. Confirm Pause/GameOver panels are readable and correctly positioned.

## Notes

- The project is intentionally limited to 16:9 for final submission.
- Settings/Pause/GameOver panels can keep semi-transparent backgrounds.
- MainPanel should remain background-free.
