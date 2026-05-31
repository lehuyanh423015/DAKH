# Enemy Facing Direction Fix

## 1. What Was Fixed
- **Enemy visual direction on spawn**: Enemies now automatically face the player when they spawn on either side of the screen.
- **Enemy visual direction after side-switch**: When SwitchEnemy or PatternEnemy3Hit lands on the opposite side of the screen, they turn around to face the player again before resuming their run animation.
- **Enemy back and die animation facing**: The `back` and `die` animations now explicitly face the player before playing, ensuring the enemy visually receives the hit from the correct direction.
- **Death drift**: Death drift logic correctly remains independent of visual facing (it always pushes the sprite away from the player).

---

## 2. Files Changed

| File | Change |
|------|--------|
| `Assets/Scripts/Enemies/EnemyAnimationController.cs` | Added `SpriteRenderer` auto-detection, `spriteFacesRightByDefault` setting, and `SetFacingDirection(int)` logic to safely flip the `SpriteRenderer.flipX` without modifying root transforms. |
| `Assets/Scripts/Enemies/Enemy.cs` | Added `GetFacingDirectionTowardPlayer()` and `UpdateVisualFacingTowardPlayer()` helpers. Called them during `Initialize()`, `TakeHit()`, `PlayDeathAnimationIfAvailable()`, and at the end of `SideSwitchRoutine()`. |

---

## 3. Inspector Checklist

Make sure every enemy prefab (NormalEnemy, HeavyEnemy, SwitchEnemy, PatternEnemy3Hit) has its `EnemyAnimationController` configured correctly:

- [ ] Each `Enemy/Visual` has a `SpriteRenderer` component.
- [ ] Each `Enemy/Visual` has an `EnemyAnimationController` component.
- [ ] `EnemyAnimationController` → **Sprite Renderer** is either assigned or left blank (it auto-detects from the `VisualRoot`).
- [ ] `EnemyAnimationController` → **Sprite Faces Right By Default**:
  - Check `true` if the raw sprite artwork for this enemy is drawn facing **right**.
  - Check `false` if the raw sprite artwork for this enemy is drawn facing **left**.
- [ ] `run`, `back`, and `die` state names are still assigned correctly.

---

## 4. How to Test

- **Spawn check**:
  - Watch a NormalEnemy spawn from the left; it should face right.
  - Watch a NormalEnemy spawn from the right; it should face left.
- **Hit reaction check (HeavyEnemy)**:
  - Hit a HeavyEnemy; it should slide backwards physically while remaining faced *towards* the player.
- **Side-switch check (SwitchEnemy / PatternEnemy3Hit)**:
  - Hit the enemy; it will teleport/slide behind the player.
  - Immediately upon landing on the new side, its sprite should flip to face the player again.
- **Death check**:
  - Deliver the fatal hit to an enemy; confirm its death animation faces the player while its death drift pushes it *away* from the player.

---

## 5. Troubleshooting

- **Enemy faces wrong direction on BOTH sides:** Your sprite artwork's default facing is likely opposite of what is set. Toggle the `Sprite Faces Right By Default` checkbox on the `EnemyAnimationController`.
- **Enemy direction is correct on spawn but wrong after side-switch:** Ensure the `SideSwitchRoutine()` in `Enemy.cs` is correctly calling `UpdateVisualFacingTowardPlayer()` immediately before finishing. (This should be working perfectly in the current code).
- **Gameplay/Collider breaks:** The code explicitly uses `SpriteRenderer.flipX` instead of scaling the root Transform by -1. This ensures that colliders and movement vectors remain 100% unaffected.
