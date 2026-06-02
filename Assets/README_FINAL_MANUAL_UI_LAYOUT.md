# Final Manual UI Layout

## Summary

MainMenu layout is now manual.

`MainPanel` and `SettingsPanel` positions are not controlled by code at runtime. Set their RectTransform values directly in Unity and keep those values as the source of truth.

## Recommended RectTransform

Use the same RectTransform setup for both `MainPanel` and `SettingsPanel`:

- Anchor Min = `(0, 0.5)`
- Anchor Max = `(0, 0.5)`
- Pivot = `(0.5, 0.5)`
- Pos X = `250`
- Pos Y = `0`
- Width = `500`
- Height = `1000`

Arrange all child UI elements manually in the Unity Editor.

## Runtime Script Behavior

`MenuPanelSwitcher` only toggles panel active state:

- `ShowSettings()` hides `MainPanel` and shows `SettingsPanel`.
- `ShowMain()` hides `SettingsPanel` and shows `MainPanel`.

It does not change:

- Anchors
- Pivot
- Anchored position
- Size delta
- Transform
- Layout
- Background Images

## Deprecated Layout Helpers

The old automatic layout helpers are disabled by default:

- `FinalMenuLayoutApplier`
- `MenuUIAutoLayout`

If either component still exists in the MainMenu scene, keep `Apply Layout At Runtime = false`, or remove the component entirely.

`FinalMenuLayoutApplier` may still disable the direct `Image` component on `MainPanel` so the large background panel does not appear. It does not modify RectTransform values.

## Safe Components

These are safe to keep:

- `CanvasResolutionSetup`: only configures `CanvasScaler`.
- `UIButtonStyler`: only styles button visuals.
- `UITextStyler`: only styles text visuals.
- `MenuPanelSwitcher`: only toggles panels.

## Panel Backgrounds

`MainPanel` should not have a large background Image.

For `MainPanel`:

- Remove `UIPanelStyler`, or
- Disable `UIPanelStyler`, or
- Set `Add Image If Missing = false`.

`UIPanelStyler` no longer auto-adds an Image on an object named `MainPanel`.

`SettingsPanel`, `PausePanel`, and `GameOverPanel` may keep their readable background Images.

## If A Panel Jumps Again

Check for any script that modifies RectTransform values:

- `anchoredPosition`
- `sizeDelta`
- `anchorMin`
- `anchorMax`
- `pivot`
- `localPosition`
- `position`

Also check Unity UI layout components such as:

- `VerticalLayoutGroup`
- `HorizontalLayoutGroup`
- `GridLayoutGroup`
- `ContentSizeFitter`
- `AspectRatioFitter`

Remove or disable any component that is not intentionally part of the manual layout.
