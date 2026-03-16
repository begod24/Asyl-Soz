# 📋 Asyl Söz — Project Analysis & Code Review

## 1. Project Overview

**Asyl Söz** is a 2D vertical endless-jumper (Doodle Jump-style) mobile game built with Unity and C#. The player collects Kazakh words while jumping upward on procedurally spawned platforms. The game includes health mechanics, spike hazards, moving platforms, a difficulty curve, and a main menu with settings/credits panels.

### Repository Contents

| Category | Items |
|---|---|
| **Scenes** | `Game.unity`, `MainMenu.unity` |
| **Prefabs** | `Platform_Normal`, `Platform_Moving`, `PlatformSpikes`, `HealthPickUp` |
| **Scripts** | 16 C# scripts across 4 folders |
| **Art** | `Background.png`, `full.png` / `empty.png` (hearts), `spikes.png` |
| **Physics** | `NoFriction.physicsMaterial2D` |
| **Rendering** | URP (Universal Render Pipeline) with 2D Renderer |
| **Input** | Unity Input System (`PlayerControls.inputactions`) |

### Folder Structure (Scripts)

```
Scripts/
├── Core/
│   └── SafeArea.cs              – Mobile safe-area handler
├── Platforms/
│   ├── DamageOnTouch.cs         – Spike damage component
│   ├── PlatformMoving.cs        – Horizontal sine-wave movement
│   ├── PlatformSpawner.cs       – Procedural row-based spawner
│   └── SpikesPlatform.cs        – Marker/tag component
├── Player/
│   ├── CameraFollowUpOnly.cs    – Upward-only camera follow
│   ├── HealthPickup.cs          – Collectible heal item
│   ├── PlayerHealth.cs          – HP system with events & persistence
│   ├── PlayerJumpController.cs  – Auto-bounce + screen wrap
│   └── PlayerMobileMove2D.cs    – Drag/tilt horizontal input
└── UI/
    ├── BackgroundScaler.cs      – Fit background to screen
    ├── GameOverController.cs    – Death/fall detection, restart
    ├── HealthUI.cs              – Heart icon display
    ├── MainMenuController.cs    – Menu panel toggling
    ├── ScoreManager.cs          – Height-based score + best
    └── SeamlessBackground.cs    – Infinite vertical scrolling BG
```

---

## 2. Script-by-Script Analysis

### Core

| Script | Lines | Purpose | Notes |
|---|---|---|---|
| `SafeArea.cs` | 31 | Adjusts a `RectTransform` to the device safe area | Clean, single-responsibility. ✅ |

### Platforms

| Script | Lines | Purpose | Notes |
|---|---|---|---|
| `DamageOnTouch.cs` | 22 | Deals damage on 2D trigger enter | Good null-check, clear logging. ✅ |
| `PlatformMoving.cs` | 23 | Sine-wave horizontal platform motion | Simple, effective. Random phase offset is nice. ✅ |
| `PlatformSpawner.cs` | 351 | Procedural row-based platform spawner | Most complex script. Well-structured with clear sections. Has a configurable difficulty curve, grace period, safe-platform guarantees, and heart spawning. ✅ |
| `SpikesPlatform.cs` | 6 | Empty marker component | Acceptable as a marker, but could be replaced by tags. |

### Player

| Script | Lines | Purpose | Notes |
|---|---|---|---|
| `CameraFollowUpOnly.cs` | 38 | Camera that only moves up, using exponential smoothing | Clean math, good approach. ✅ |
| `HealthPickup.cs` | 25 | Pickup that heals the player and self-destructs | Simple, correct. ✅ |
| `PlayerHealth.cs` | 109 | Full HP system with events, i-frames, and `PlayerPrefs` persistence | Well-designed event-driven architecture. ✅ |
| `PlayerJumpController.cs` | 67 | Auto-bounce on platform landing + horizontal screen wrap | Good top-landing check with configurable normal threshold. ✅ |
| `PlayerMobileMove2D.cs` | 114 | Drag or tilt horizontal movement using Input System | Good mode switching. ✅ |

### UI

| Script | Lines | Purpose | Notes |
|---|---|---|---|
| `BackgroundScaler.cs` | 24 | Scales a sprite to fill the camera view | Clean, focused. ✅ |
| `GameOverController.cs` | 107 | Detects death (fall or HP) and shows game-over UI | Event subscription pattern is correct. ✅ |
| `HealthUI.cs` | 54 | Displays filled/empty heart icons | Proper subscribe/unsubscribe pattern. ✅ |
| `MainMenuController.cs` | 74 | Panel toggling and scene loading | Works, but auto-find by `GameObject.Find` is fragile. ⚠️ |
| `ScoreManager.cs` | 76 | Tracks height as score with best-score persistence | Clean and simple. ✅ |
| `SeamlessBackground.cs` | 73 | Infinite vertical background scrolling with parallax | Solid wrap logic. ✅ |

---

## 3. Game State Assessment

### What Works Well ✅

- **Core gameplay loop is complete**: jump → climb → collect → die → restart.
- **Difficulty curve** is thoughtfully implemented with grace periods, ramping hazards, and capped row gaps.
- **Health system** is robust with i-frames, events, persistence, and clear separation of concerns.
- **Mobile input** supports both drag and tilt modes through Unity's new Input System.
- **Safe area** handling is included from the start, showing mobile-first awareness.
- **Platform spawning** guarantees at least one safe platform per row, preventing unwinnable states.

### What's Missing or Incomplete ⚠️

- **Kazakh word collection** (the core theme) is not yet implemented — no word data, no `ScriptableObjects`, no word UI, no combo system.
- **No audio** — no sound effects or background music.
- **No settings functionality** — Settings panel exists in the menu but has no actual options (volume, controls, language).
- **No pause menu** in-game.
- **No animations** or visual feedback for jumping, damage, or collecting items.
- **No particle effects** for landing, damage, or pickups.
- **No tutorial or onboarding** for first-time players.

---

## 4. Programming Feedback

### Strengths 💪

1. **Good folder organization** — Scripts are logically separated into `Core`, `Platforms`, `Player`, and `UI`.
2. **Event-driven health system** — `PlayerHealth` uses `Action<int, int>` events, and both `HeartsUI` and `GameOverController` subscribe/unsubscribe correctly (`OnEnable`/`OnDisable`).
3. **Configurable via Inspector** — Almost every tunable value is exposed as a `[SerializeField]` with `[Header]` and `[Tooltip]` attributes, making the game designer-friendly.
4. **Null-safety** — Most scripts include null checks before using references, and use `FindGameObjectWithTag` as a fallback.
5. **Physics-based jump calibration** — `PlatformSpawner.CalibrateJumpHeight()` automatically calculates max jump height from gravity and jump force, so row gaps remain reachable even when physics constants change.
6. **Single-responsibility** — Most scripts do one thing well (e.g., `BackgroundScaler`, `CameraFollowUpOnly`, `DamageOnTouch`).

### Issues & Suggestions for Improvement 🔧

#### A. Clean Code

| Issue | Location | Suggestion |
|---|---|---|
| **Inconsistent indentation** in `TakeDamage()` | `PlayerHealth.cs:41-57` | The method body uses different indentation than the rest of the file. Normalize to 4-space or 1-tab consistently. |
| **Redundant `UnityEngine.` prefix** on `[SerializeField]` | Multiple scripts | `[UnityEngine.SerializeField]` is unnecessary when `using UnityEngine;` is already at the top. Replace with `[SerializeField]`. |
| **File names don't match class names** | `DamageOnTouch.cs` → `DamageOnTouch2D`, `HealthPickup.cs` → `HealthPickup2D`, `PlatformMoving.cs` → `MovingPlatform`, `HealthUI.cs` → `HeartsUI`, `SeamlessBackground.cs` → `SeamlessBackgroundY` | File names should match their class names. This is a Unity convention and avoids confusion. |
| **Comments in Russian** | `PlayerHealth.cs:88, 103`, `PlayerJumpController.cs:23` | Use English for code comments to keep the codebase accessible to all contributors. |
| **Debug.Log left in production code** | `DamageOnTouch.cs:20`, `PlayerHealth.cs:50,54`, `MainMenuController.cs` (everywhere) | Wrap debug logging in `#if UNITY_EDITOR` or use a centralized logging utility, so logs don't appear in release builds. |
| **Magic numbers** | `PlatformSpawner.cs:142` (`1.4f`), `PlatformSpawner.cs:185-186` (`0.22f`, `0.18f`) | Extract to named constants or `[SerializeField]` fields with descriptive names. |

#### B. OOP & Architecture

| Issue | Suggestion |
|---|---|
| **No interfaces or abstractions** | Introduce an `IDamageable` interface (with `TakeDamage(int)`) so the damage system isn't tightly coupled to `PlayerHealth`. This allows future enemies or destructible objects. |
| **No base class for platforms** | `MovingPlatform`, `SpikesPlatform`, and normal platforms share no common base. Consider a `PlatformBase` class to centralize shared behavior (e.g., pooling, destruction). |
| **`PlatformSpawner` is a God Class** | At 351 lines, it handles spawning, difficulty, X-position generation, heart spawning, and despawning. Consider splitting into: `DifficultyManager`, `PlatformPool`, and `PlatformSpawner` (only spawn logic). |
| **No object pooling** | Platforms are `Instantiate`/`Destroy`-ed every frame. For a mobile game, this causes GC spikes. Use Unity's `ObjectPool<T>` or a custom pool. |
| **`PlayerPrefs` for game state** | `PlayerHealth` and `ScoreManager` persist state with `PlayerPrefs`. This is fragile and doesn't scale. Consider a `GameStateManager` (singleton or ScriptableObject-based) that centralizes save/load. |
| **`FindObjectOfType` / `GameObject.Find` usage** | `MainMenuController.cs:15-16`, `GameOverController.cs:37` | These are expensive runtime lookups. Prefer explicit Inspector assignment or a service locator / dependency injection pattern. |
| **No ScriptableObjects for game data** | The README mentions them, but none exist yet. Word data, difficulty curves, and platform configurations should be `ScriptableObject` assets to separate data from logic. |

#### C. Performance (Mobile-Critical)

| Issue | Suggestion |
|---|---|
| **`PlayerPrefs.Save()` called on every health change and every frame (score)** | Batch saves — e.g., only save on game over or scene transitions. `PlayerPrefs.Save()` triggers a file write and can hitch on mobile. |
| **No object pooling** | `Instantiate`/`Destroy` generates garbage. Use pooling for platforms and pickups. |
| **`Camera.main` called in `Awake`** | This is fine, but note that `Camera.main` was slow prior to Unity 2020. Since URP is used, it should be cached (which it is ✅). |

#### D. Safety & Edge Cases

| Issue | Location | Suggestion |
|---|---|---|
| **Division by zero risk** | `PlayerMobileMove2D.cs:86` — `Screen.width * dragSensitivity` | If `dragSensitivity` is 0, this divides by zero. Add a guard or clamp `dragSensitivity` to a minimum. |
| **Gravity fallback hides misconfiguration** | `PlatformSpawner.cs:134` | The fallback `gravity = 9.81f` silently masks a missing Rigidbody2D gravity scale. Consider logging a warning. |
| **No `maxHealth` validation** | `PlayerHealth.cs` | If `maxHealth` is set to 0 or negative in the Inspector, the health system breaks silently. Validate in `Awake()`. |

---

## 5. Summary

| Area | Rating | Comment |
|---|---|---|
| **Project Structure** | ⭐⭐⭐⭐ | Clean folder layout, logical separation |
| **Code Quality** | ⭐⭐⭐ | Readable but has naming inconsistencies and leftover debug logs |
| **OOP / Architecture** | ⭐⭐⭐ | Good use of events, but lacks interfaces, base classes, and pooling |
| **Game Completeness** | ⭐⭐ | Core jump mechanics work; theme (Kazakh words) not yet implemented |
| **Mobile Readiness** | ⭐⭐⭐ | Safe area + input modes are good; needs pooling and save optimization |
| **Documentation** | ⭐⭐⭐⭐ | README is well-written and descriptive |

### Top 5 Actionable Recommendations

1. **Fix file/class naming mismatches** — Rename files to match class names (or vice versa) to follow Unity conventions.
2. **Implement object pooling** — Replace `Instantiate`/`Destroy` with pooling for platforms and pickups to avoid GC spikes on mobile.
3. **Add the Kazakh word system** — Create `ScriptableObject`-based word data, a word spawner, a collection UI, and the combo mechanic described in the README.
4. **Extract `PlatformSpawner` into smaller classes** — Split difficulty calculation, position generation, and object lifecycle into separate responsibilities.
5. **Clean up `[UnityEngine.SerializeField]` to `[SerializeField]`** and normalize indentation across all files for consistency.
