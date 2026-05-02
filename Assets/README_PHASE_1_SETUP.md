# Phase 1 – Setup & Verification Guide

## 1. What Was Implemented

| File | Responsibility |
|------|----------------|
| `Assets/Scripts/Core/GameManager.cs` | Singleton that owns the game-over state. Exposes `IsGameOver` (bool property) and `GameOver()` (one-shot method that logs "Game Over" to the Console). |
| `Assets/Scripts/Enemies/Enemy.cs` | Moves toward the player each frame. Knows its spawn side (`Left`/`Right`). Stops when game is over. Calls `GameManager.GameOver()` via `OnTriggerEnter2D` when it touches the player. |
| `Assets/Scripts/Spawning/EnemySpawner.cs` | Spawns enemy prefabs at a configurable interval. Randomly picks left or right. Calls `Enemy.Initialize()` to pass the player reference and spawn side. Stops when game is over. |
| `Assets/Scripts/Player/PlayerCombat.cs` | Reads `A`/`LeftArrow` (attack left) and `D`/`RightArrow` (attack right). Finds the closest in-range enemy on the correct side using `FindObjectsByType<Enemy>()` and destroys it. |

---

## 2. Scene Object Checklist

Complete every item before pressing Play:

### GameManager
- [ ] `GameManager` empty GameObject exists in MainScene.
- [ ] `GameManager.cs` is attached to it.

### Player
- [ ] `Player` GameObject exists at center position `(0, 0, 0)`.
- [ ] Player tag is set to **`Player`** *(required for Enemy collision detection)*.
- [ ] Player has a **Box Collider 2D**.
- [ ] Player Box Collider 2D → **Is Trigger = ✓ (enabled)**.
- [ ] `PlayerCombat.cs` is attached to Player.

### EnemySpawner
- [ ] `EnemySpawner` empty GameObject exists in MainScene.
- [ ] `EnemySpawner.cs` is attached to it.
- [ ] **Enemy Prefab** slot → drag `Assets/Prefabs/Enemy.prefab` into it.
- [ ] **Player Transform** slot → drag the `Player` GameObject into it.

### Enemy Prefab
- [ ] `Assets/Prefabs/Enemy.prefab` exists.
- [ ] `Enemy.cs` is attached to the prefab.
- [ ] Prefab has a **Sprite Renderer** (with a visible sprite so you can see it).
- [ ] Prefab has a **Box Collider 2D** → Is Trigger = ✗ (not a trigger).
- [ ] Prefab has a **Rigidbody 2D** → Body Type = **Kinematic**.

> **Why this collider setup?**
> Unity fires `OnTriggerEnter2D` when *at least one* collider is a trigger and *at least one* object has a Rigidbody.
> The player's collider is the trigger; the enemy's kinematic Rigidbody satisfies the Rigidbody requirement.
> The event is received on any script on either object — here, `Enemy.cs` on the enemy detects the overlap.

---

## 3. Inspector Values to Set

### PlayerCombat (on Player)
| Field | Recommended Value |
|-------|-------------------|
| `attackRange` | `1.5` – `2.0` |

### EnemySpawner (on EnemySpawner object)
| Field | Recommended Value |
|-------|-------------------|
| `spawnInterval` | `1.5` |
| `leftSpawnPosition` | `(-7, 0, 0)` |
| `rightSpawnPosition` | `(7, 0, 0)` |

### Enemy (on Enemy prefab)
| Field | Recommended Value |
|-------|-------------------|
| `moveSpeed` | `2.0` – `3.0` |

---

## 4. How to Test

1. **Press Play** in the Unity Editor.
2. Confirm enemies appear from the left and/or right sides at ~1.5-second intervals.
3. Confirm each enemy moves toward the center (Player position).
4. When an enemy is close on the **left** side, press **A** or **Left Arrow**.
   - ✅ The enemy should be destroyed instantly.
5. When an enemy is close on the **right** side, press **D** or **Right Arrow**.
   - ✅ The enemy should be destroyed instantly.
6. Press the wrong direction key or let an enemy get past your range.
   - ✅ Nothing happens (no false-positive destroy).
7. Let an enemy walk all the way into the Player without pressing anything.
   - ✅ The Console should print **"Game Over"**.
   - ✅ All surviving enemies should freeze in place.
   - ✅ No new enemies should spawn.

---

## 5. Known Limitations (Phase 1)

- ❌ No score system yet.
- ❌ No combo system yet.
- ❌ No miss / wrong-direction penalty yet.
- ❌ No stun or recovery window yet.
- ❌ No in-game UI (no HUD, no game-over screen) yet.
- ❌ No animations yet.
- ❌ No sound / audio yet.
- ❌ No restart flow yet (must press Stop → Play again in the Editor).

---

## 6. Phase 2 – Next Steps

| Feature | Notes |
|---------|-------|
| **Score** | Increment on each successful kill. Display with a TextMeshPro canvas. |
| **Combo** | Track consecutive kills; reset on miss or game over. |
| **Miss detection** | If player presses a direction but no valid enemy is in range, log/penalise. |
| **Short stun** | On a miss, briefly disable player input (e.g. 0.5 s) to add risk. |
| **Game Over UI** | Show a canvas panel with final score and a Restart button. |
| **Restart flow** | `SceneManager.LoadScene(SceneManager.GetActiveScene().name)` on button click. |
| **Difficulty scaling** | Gradually increase `spawnInterval` reduction and `moveSpeed` over time. |
