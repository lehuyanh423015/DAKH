# Phase Audio - Background Music Setup

## What Was Added

- `SceneMusicController` for scene-local looping background music.
- MainMenu music support.
- Gameplay music support.
- GameOver can keep gameplay music playing while `Time.timeScale` is frozen.
- Restarting gameplay reloads the scene and replays gameplay music from the beginning.

## Files Created/Changed

- `Assets/Scripts/Audio/SceneMusicController.cs`
- `Assets/README_PHASE_AUDIO_BGM_SETUP.md`

No gameplay logic or existing SFX playback was changed.

## Manual Setup Checklist

### MainMenu

- [ ] Create a `MainMenuMusic` GameObject in the `MainMenu` scene.
- [ ] Add an `AudioSource`.
- [ ] Add `SceneMusicController`.
- [ ] Assign the menu music clip to `Music Clip`.
- [ ] Set `Loop = true`.
- [ ] Set `Play On Start = true`.
- [ ] Set `Restart From Beginning On Start = true`.
- [ ] Set `Keep Playing On Game Over = true`.
- [ ] Set `Pause On Game Pause = false`.
- [ ] Set `Volume` between `0.5` and `0.7`.

Do not make `MainMenuMusic` persistent. It should unload when gameplay loads.

### Gameplay

- [ ] Create a `GameplayMusic` GameObject in the gameplay scene.
- [ ] Add an `AudioSource`.
- [ ] Add `SceneMusicController`.
- [ ] Assign the gameplay music clip to `Music Clip`.
- [ ] Set `Loop = true`.
- [ ] Set `Play On Start = true`.
- [ ] Set `Keep Playing On Game Over = true`.
- [ ] Set `Restart From Beginning On Start = true`.
- [ ] Set `Pause On Game Pause = false` by default.
- [ ] Set `Volume` between `0.5` and `0.7`.

On GameOver, `Time.timeScale` becomes `0`, but the AudioSource keeps playing unless `Pause On Game Pause` or `Keep Playing On Game Over` settings are changed.

## Existing AudioManager Compatibility

- `AudioManager` continues to handle short SFX with `PlayOneShot`.
- `SceneMusicController` handles background music through a separate `AudioSource`.
- Do not play background music through `AudioManager.PlayOneShot`.

## Settings Volume Compatibility

The current `SettingsManager` controls `AudioListener.volume` as master volume. This is acceptable for now and affects both SFX and music.

Separate music and SFX sliders are not included in this phase.

## Recommended Audio Import Settings

### Music

- `Load Type = Streaming` or `Compressed In Memory`.
- `Compression Format = Vorbis` if available.
- `Preload Audio Data` can be off for large music files.

### SFX

- `Load Type = Decompress On Load`.
- Short files can be WAV, OGG, or MP3.

## How To Test MainMenu Music

1. Open `MainMenu`.
2. Press Play in the Unity Editor.
3. Confirm menu music starts.
4. Open Settings and go Back.
5. Confirm menu music continues.
6. Click Play.
7. Confirm the gameplay scene loads and menu music stops naturally.

## How To Test Gameplay Music

1. Enter the gameplay scene.
2. Confirm gameplay music starts from the beginning.
3. Trigger GameOver.
4. Confirm gameplay music continues playing.
5. Click Restart.
6. Confirm the scene reloads and gameplay music starts from the beginning.
7. Pause the game if available.
8. Confirm music behavior matches the `Pause On Game Pause` setting.

## Known Limitations

- No separate music/SFX sliders yet.
- No crossfade.
- No dynamic music.
- No music transition delay.
- Scene-local music only.
