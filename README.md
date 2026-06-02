# DAKH - Unity 2D Timing Combat Game

DAKH là game 2D timing-combat được xây dựng bằng Unity. Người chơi đứng ở trung tâm màn hình, quan sát kẻ địch tiến đến từ hai phía trái/phải và tấn công đúng hướng, đúng thời điểm để ghi điểm, duy trì combo và đạt kỷ lục cao nhất.

## Trạng Thái Hiện Tại

Dự án hiện đã có một gameplay loop hoàn chỉnh và có thể build/demo:

- Main Menu có AI gameplay demo chạy phía sau UI.
- Gameplay scene có player, enemy, spawn, combat, score, combo và game over.
- Có nhiều loại enemy với hành vi khác nhau.
- Có Pause Menu, Settings Menu và Game Over Panel.
- Có animation, hit effect, camera shake, SFX và background music.
- Có highest score và highest combo lưu bằng `PlayerPrefs`.
- UI được chuẩn hóa cho tỉ lệ 16:9, hỗ trợ `1280x720` và `1920x1080`.

## Công Nghệ Sử Dụng

- Unity
- C#
- Unity UI / UGUI
- TextMeshPro
- Universal Render Pipeline 2D
- Unity 2D Animation
- PlayerPrefs

## Gameplay

Người chơi không di chuyển tự do, mà đứng ở khu vực trung tâm. Kẻ địch sẽ spawn từ bên trái hoặc bên phải và tiến về phía player.

Người chơi cần bấm đúng hướng tấn công:

| Phím | Hành động |
|---|---|
| `A` / `Left Arrow` | Tấn công trái |
| `D` / `Right Arrow` | Tấn công phải |
| `Esc` | Pause / Resume |

Nếu tấn công đúng hướng và enemy nằm trong tầm đánh, enemy sẽ bị trúng đòn. Nếu đánh trượt, player bị stun, trừ khi đang có combo shield.

Chuỗi animation attack 1/2/3 chỉ là chuỗi đòn đánh liên tiếp trong một khoảng thời gian. Mỗi lần bấm tấn công vẫn phải kiểm tra hit/miss riêng. Nếu trượt ở nhịp 2 hoặc nhịp 3, player vẫn bị stun như bình thường.

## Hệ Thống Điểm Và Combo

- `Score`: tăng khi enemy bị tiêu diệt.
- `Combo`: tăng mỗi khi đánh trúng chính xác.
- `Combo Timeout`: combo reset nếu quá thời gian quy định mà không hit tiếp.
- `Combo Shield`: đạt ngưỡng combo nhất định sẽ nhận shield; shield có thể tha một lần miss.
- `Highest Score`: điểm cao nhất được lưu lại.
- `Highest Combo`: combo cao nhất được lưu lại.

## Enemy

Dự án hiện có các enemy prefab chính:

- `NormalEnemy`: enemy cơ bản.
- `HeavyEnemy`: enemy cần nhiều hit hơn.
- `SwitchEnemy`: enemy có thể đổi bên sau khi bị đánh.
- `PatternEnemy3Hit`: enemy yêu cầu nhiều hit và có hành vi đổi bên/di chuyển đặc biệt.

Enemy có các cơ chế bổ sung:

- Di chuyển về phía player.
- Knockback khi bị hit nhưng chưa chết.
- Lane blocking để hạn chế enemy chồng lên nhau.
- Death animation.
- Không gây game over khi đã bị đánh bại nhưng đang chờ animation chết.

## UI Và Menu

Dự án có các màn hình/UI chính:

- `MainMenu`: màn hình chính, có gameplay demo AI phía sau.
- `SettingsPanel`: tùy chỉnh fullscreen, resolution và master volume.
- `Gameplay HUD`: hiển thị score, combo, shield, highest score và highest combo.
- `PausePanel`: tạm dừng game bằng `Time.timeScale = 0`.
- `GameOverPanel`: hiển thị kết quả, restart và các hành động sau khi thua.

UI được thiết kế cho 16:9. Các panel trong Main Menu được sắp xếp thủ công trong Unity, không bị script tự động thay đổi vị trí runtime.

## Audio

Dự án tách SFX và background music:

- `AudioManager`: quản lý SFX ngắn bằng `PlayOneShot`.
- `SceneMusicController`: quản lý nhạc nền theo scene.

Hỗ trợ:

- Nhạc Main Menu.
- Nhạc gameplay.
- Nhạc Game Over riêng.
- SFX hit, miss, stun, shield, restart và game over.

## Cấu Trúc Thư Mục Quan Trọng

```text
Assets/
|-- Animations/          Animation clips và animator controllers
|-- Art/                 Sprite, background, visual assets
|-- Audio/               Music và SFX
|-- Prefabs/             Enemy prefabs và effect prefabs
|-- Scenes/              MainMenu và SampleScene
|-- Scripts/
|   |-- Audio/           AudioManager, SceneMusicController
|   |-- Camera/          Camera follow
|   |-- Core/            GameManager, DifficultyManager
|   |-- Demo/            DemoPlayerAI
|   |-- Enemies/         Enemy logic và animation controller
|   |-- Environment/     Background/video helpers
|   |-- Feedback/        Camera shake, temporary effects
|   |-- Player/          Combat, movement, animation
|   |-- Spawning/        EnemySpawner
|   |-- UI/              GameUI, menu, pause, settings, UI styler
|   `-- Visual/          VisualRoot helper
`-- Settings/            URP/renderer settings
```

## Các Script Chính

| Script | Vai trò |
|---|---|
| `GameManager` | Quản lý trạng thái game, score, combo, high score, game over |
| `PlayerCombat` | Xử lý input, hit/miss, stun, shield và attack |
| `PlayerMovement` | Tạo dịch chuyển nhẹ theo hướng tấn công |
| `PlayerAnimationController` | Điều khiển animation player và chuỗi attack 1/2/3 |
| `Enemy` | Di chuyển, nhận damage, knockback, side-switch, death |
| `EnemySpawner` | Spawn enemy theo thời gian và weight |
| `DifficultyManager` | Tăng độ khó theo thời gian |
| `GameUI` | Cập nhật HUD, game over panel, restart |
| `PauseMenuManager` | Pause/resume/restart/back to menu |
| `SettingsManager` | Resolution, fullscreen, volume |
| `AudioManager` | SFX |
| `SceneMusicController` | Background music theo scene |
| `DemoPlayerAI` | AI demo trên Main Menu |

## Scene

| Scene | Mô tả |
|---|---|
| `Assets/Scenes/MainMenu.unity` | Main Menu, settings và demo gameplay nền |
| `Assets/Scenes/SampleScene.unity` | Gameplay chính |

Trong `ProjectSettings/EditorBuildSettings.asset`, cả hai scene đều được thêm vào build.

## Thiết Lập UI/Build Khuyến Nghị

Dự án target 16:9:

- `1280x720`
- `1920x1080`

Khuyến nghị trong Player Settings:

- Fullscreen Mode: `Windowed`
- Default Width: `1280`
- Default Height: `720`
- Resizable Window: `false`

Canvas nên có:

- `CanvasScaler.uiScaleMode = Scale With Screen Size`
- Reference Resolution: `1920x1080`
- Match Width Or Height: `0.5`

Có thể thêm `CanvasResolutionSetup` vào Canvas để tự động áp dụng các giá trị trên.

## Cách Chạy Dự Án

1. Mở project bằng Unity.
2. Mở scene `Assets/Scenes/MainMenu.unity`.
3. Bấm Play để test từ Main Menu.
4. Bấm `PLAY` để vào gameplay.
5. Test các chức năng:
   - Tấn công trái/phải.
   - Combo và score.
   - Pause bằng `Esc`.
   - GameOver và Restart.
   - Settings resolution/volume.

## Cách Build

1. Mở `File > Build Settings`.
2. Kiểm tra scene build gồm:
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/SampleScene.unity`
3. Chọn target platform Windows.
4. Build với resolution 16:9.
5. Test build ở `1280x720` và `1920x1080`.

## Các Tính Năng Đã Hoàn Thành

- Gameplay 2D timing-combat.
- Enemy spawn trái/phải.
- Hit/miss/stun.
- Combo và combo shield.
- Highest score và highest combo.
- Nhiều enemy type.
- Difficulty scaling.
- Animation player/enemy.
- Hit effect và camera shake.
- Main Menu có AI demo.
- Pause, Settings, GameOver.
- SFX và background music.
- Hỗ trợ background video.
- UI polish cho bản build cuối.
- 16:9 UI/build setup.

## Hạn Chế Hiện Tại

- Chưa có leaderboard online.
- Chưa có save/load progression phức tạp.
- Chưa có shop hoặc upgrade system.
- Chưa có mobile control.
- Chưa có nhiều màn chơi/boss riêng.
- Chưa tách riêng slider Music/SFX volume.

## Hướng Phát Triển Tương Lai

- Thêm tutorial.
- Thêm boss/wave system.
- Thêm enemy pattern mới.
- Thêm object pooling để tối ưu spawn/destroy enemy.
- Thêm leaderboard.
- Thêm mobile support.
- Thêm tùy chỉnh riêng music volume và SFX volume.
- Thêm nhiều background/video theme hơn.

## Ghi Chú Cho Phát Triển

- Không nên sửa trực tiếp file `.csproj` do Unity tự sinh lại.
- Nếu VS Code C# Dev Kit báo project không hỗ trợ SDK-style, đây là lỗi tooling, không phải lỗi gameplay.
- Gameplay scene sử dụng `Time.timeScale = 0` khi game over/pause; audio vẫn có thể tiếp tục chạy vì `AudioSource` không phụ thuộc timeScale.
- MainMenu layout hiện được sắp xếp thủ công trong Unity; không nên bật lại các script auto layout cũ nếu không cần.
