# Phase 19 – Asset Replacement Guide

## 1. What Was Added in Phase 19
- **VisualRoot Script**: A new script to identify the visual child object of an entity.
- **Root-vs-Visual Architecture**: Established a standard way to separate gameplay logic (root) from visual art/animation (child).
- **Safe Asset Replacement Workflow**: Defined step-by-step processes to replace placeholder art with real assets without breaking collision or combat logic.
- **Child Visual Support**: `Enemy.cs` and `PlayerCombat.cs` updated to prioritize child visuals for damage flashing and stuns while gracefully falling back to the root if not yet set up.

---

## 2. Files Created / Changed
- `Assets/Scripts/Visual/VisualRoot.cs` (Created)
- `Assets/Scripts/Enemies/Enemy.cs` (Updated)
- `Assets/Scripts/Player/PlayerCombat.cs` (Updated)

---

## 3. Core Principle
The core philosophy of this architecture is **Separation of Concerns**:
- **Gameplay Root Object**: Controls logic, state, Rigidbody2D, and Collider2D.
- **Visual Child Object**: Controls SpriteRenderer, Animator, and visual-only transforms (scale, color, rotation).

**Rules:**
- Do not move colliders or gameplay scripts to the visual children.
- Do not change `attackRange` or timing values when swapping cosmetic art.

---

## 4. Recommended Player Hierarchy

```text
Player                         ← Gameplay Root (PlayerCombat.cs, BoxCollider2D)
├── Visual                     ← Visual Child (VisualRoot.cs, SpriteRenderer, Animator)
├── LeftAttackIndicator
└── RightAttackIndicator
```

- **Player**: Holds the BoxCollider2D, Rigidbody2D, `PlayerCombat.cs`, and `PlayerMovement.cs`.
- **Visual**: Holds `VisualRoot.cs`, `SpriteRenderer`, and eventually an `Animator`.

---

## 5. Recommended Enemy Hierarchy

```text
EnemyRoot                      ← Gameplay Root (Enemy.cs, BoxCollider2D, Rigidbody2D)
└── Visual                     ← Visual Child (VisualRoot.cs, SpriteRenderer, Animator)
```

- **EnemyRoot**: Holds `Enemy.cs`, BoxCollider2D, and Rigidbody2D.
- **Visual**: Holds `VisualRoot.cs`, `SpriteRenderer`, and eventually an `Animator`.

---

## 6. How to Replace Player Sprite

1. In the Hierarchy, right-click the **Player** object and select **Create Empty**. Name it `Visual`.
2. Add a **SpriteRenderer** component to the `Visual` object.
3. Add the **VisualRoot.cs** script to the `Visual` object.
4. Assign your real player sprite to the `SpriteRenderer`.
5. Remove or disable the placeholder `SpriteRenderer` on the **Player** root object.
6. Make sure the **BoxCollider2D** stays on the **Player** root object. Adjust its size to match the new gameplay bounds (not necessarily pixel-perfect to the art).
7. (Optional) Drag the `Visual` object into the `PlayerVisualRoot` slot on the `PlayerCombat` script. (It auto-detects if you forget).

---

## 7. How to Replace Enemy Sprite

1. Double-click an Enemy prefab to open it.
2. Right-click the root object and select **Create Empty**. Name it `Visual`.
3. Add a **SpriteRenderer** component to the `Visual` object.
4. Add the **VisualRoot.cs** script to the `Visual` object.
5. Assign your real enemy sprite to the `SpriteRenderer`.
6. Remove or disable the placeholder `SpriteRenderer` on the root object.
7. Keep `Enemy.cs`, `BoxCollider2D`, and `Rigidbody2D` on the root object.
8. Adjust the `BoxCollider2D` size on the root to define the hitbox.
9. Test the enemy in-game to ensure knockbacks and stuns still work. Repeat for Heavy, Switch, and Pattern3 enemies.

---

## 8. How to Handle Animations Later

- When you have animation assets, add the **Animator** component to the `Visual` child object.
- Keep all gameplay logic on the root object.
- **Critical:** Animation clips should **not** drive the hit detection or combat timing. Attack timing remains strictly code-controlled by `PlayerCombat.cs`. Animations should sync to the code, not the other way around.

---

## 9. How to Handle Weapons Later

- Weapon art can be added as another child object under `Player` or as a child of `Player/Visual`.
- Weapons should **not** alter `attackRange`, `stunDuration`, input response, or combo timing unless explicitly programmed as a new mechanic later.
- Swapping weapons should primarily change the sprite, the hit effect, and the sound effect.

---

## 10. Testing Checklist

After restructuring a prefab into the Root-Visual format:
- [ ] Player stun color tint still works.
- [ ] Enemy damage darkening tint still works.
- [ ] NormalEnemy still moves and dies correctly.
- [ ] HeavyEnemy still takes knockback.
- [ ] SwitchEnemy still flips to the opposite side safely.
- [ ] PatternEnemy3Hit still alternates sides.
- [ ] Colliders still trigger Game Over when an enemy touches the player.
- [ ] Attack hit detection range feels accurate.
- [ ] UI (Score/Combo/Shield) still works.
- [ ] Restart button still works.

---

## 11. Known Limitations

- No final art is included in this phase.
- No `Animator` controllers have been created yet.
- No complex weapon system exists.
- No advanced particle polish is included.
- Inserting real assets will require manual `BoxCollider2D` tuning to ensure the game feels fair.

---

## 12. Next Phase Suggestion

- **Phase 20:** Build/export a playable version to share.
- **Alternatively Phase 20:** Implement a Main Menu and a Pause Menu before final packaging.
- **Alternatively Phase 20:** Replace placeholder art once your final assets are ready to integrate.
