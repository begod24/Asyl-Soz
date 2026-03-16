# 🔧 Asyl Söz — Refactor Log

Step-by-step explanation of every change, what was done, how, and why.

---

## Step 1: Fix File/Class Name Mismatches

### What was done
Renamed 5 script files so their names match the C# class they contain:

| Old File Name | Class Inside | New File Name |
|---|---|---|
| `DamageOnTouch.cs` | `DamageOnTouch2D` | `DamageOnTouch2D.cs` |
| `PlatformMoving.cs` | `MovingPlatform` | `MovingPlatform.cs` |
| `HealthPickup.cs` | `HealthPickup2D` | `HealthPickup2D.cs` |
| `HealthUI.cs` | `HeartsUI` | `HeartsUI.cs` |
| `SeamlessBackground.cs` | `SeamlessBackgroundY` | `SeamlessBackgroundY.cs` |

### Why this is better
Unity serializes MonoBehaviour references using the file name. When the file name doesn't match the class name, Unity can fail to find the script, the Inspector shows "Missing Script" errors, and developers waste time searching for the wrong file. This is a standard Unity convention.

### How it was done
Renamed each `.cs` file and its corresponding `.meta` file together, preserving the GUID so that prefab/scene references stay intact.

---

## Step 2: Clean Up `[UnityEngine.SerializeField]` → `[SerializeField]`

### What was done
Replaced every `[UnityEngine.SerializeField]` with `[SerializeField]` across all scripts.

### Why this is better
Every file already has `using UnityEngine;` at the top, making the `UnityEngine.` prefix redundant. Removing it:
- Reduces visual noise
- Follows standard C#/Unity coding conventions
- Makes the code consistent (some files used `[SerializeField]` and others used the long form)

### Files changed
`CameraFollowUpOnly.cs`, `HealthPickup2D.cs`, `PlayerHealth.cs`, `PlayerJumpController.cs`, `PlayerMobileMove2D.cs`, `GameOverController.cs`, `HeartsUI.cs`, `MainMenuController.cs`, `ScoreManager.cs`, `SeamlessBackgroundY.cs`, `MovingPlatform.cs`, `DamageOnTouch2D.cs`

---

## Step 3: Translate Russian Comments to English

### What was done
Translated all Russian comments to English:
- `PlayerHealth.cs:87` — `"если вдруг сохранено 0..."` → `"If saved value is 0, start with at least 1 HP"`
- `PlayerHealth.cs:103` — `"Вызывай перед Restart..."` → XML doc `"Call before Restart so we don't load stale or zero HP"`
- `PlayerJumpController.cs:23` — `"Это нужно PlatformSpawner..."` → XML doc `"Exposed for PlatformSpawner jump-height auto-calibration."`

### Why this is better
English is the standard language for code comments in open-source and collaborative projects. Mixed-language comments confuse contributors and tools (linters, documentation generators).

---

## Step 4: Remove Debug.Log from Production Code

### What was done
- **`DamageOnTouch2D.cs`**: Wrapped the warning in `#if UNITY_EDITOR`. Removed the `Debug.Log` that logged every spike hit.
- **`PlayerHealth.cs`**: Removed `Debug.Log` calls from `TakeDamage()` and death event.
- **`MainMenuController.cs`**: Removed all `Debug.Log` calls from button handlers. Also removed `Debug.LogWarning` that logged missing panels.

### Why this is better
`Debug.Log` is expensive on mobile — each call allocates strings and triggers the logger. Leftover logs in release builds:
- Pollute the logcat output
- Consume CPU cycles
- May leak gameplay state information

The `#if UNITY_EDITOR` guard ensures warnings only appear during development.

---

## Step 5: Fix Indentation in `PlayerHealth.TakeDamage()`

### What was done
The `TakeDamage()` method had its body at column 0 (no indentation) while the rest of the file used 4-space indentation. Normalized to 4-space.

### Why this is better
Consistent indentation is the most basic readability requirement. Inconsistent indentation makes code harder to scan and can cause merge conflicts.

---

## Step 6: Add `IDamageable` Interface

### What was done
Created `Scripts/Core/IDamageable.cs`:
```csharp
public interface IDamageable
{
    void TakeDamage(int amount);
}
```
`PlayerHealth` now implements `IDamageable`, and `DamageOnTouch2D` references `IDamageable` instead of `PlayerHealth` directly.

### Why this is better
**Open/Closed Principle**: Without the interface, `DamageOnTouch2D` was hardcoded to damage only `PlayerHealth`. Now any object that implements `IDamageable` (future enemies, destructible crates, shields) automatically works with the spike system — no code changes needed.

**Testability**: You can mock `IDamageable` in unit tests without needing a full `PlayerHealth` MonoBehaviour.

---

## Step 7: Add `maxHealth` Validation

### What was done
Added `ValidateConfig()` in `PlayerHealth.Awake()`:
- If `maxHealth <= 0`, logs an error and defaults to 3.
- Clamps `startHealth` between 1 and `maxHealth`.

### Why this is better
Previously, setting `maxHealth = 0` in the Inspector would silently break the health system (division by zero in UI, instant death). Now misconfiguration is caught early with a clear error message.

---

## Step 8: Add Division-by-Zero Guard in `PlayerMobileMove2D`

### What was done
Added `dragSensitivity = Mathf.Max(dragSensitivity, 0.01f)` in `Awake()`.

### Why this is better
`ReadDragX()` divides by `Screen.width * dragSensitivity`. If a designer sets `dragSensitivity = 0` in the Inspector, this causes a division-by-zero → `NaN` → the player teleports to infinity. The clamp prevents this silently.

---

## Step 9: Extract `DifficultyManager` from `PlatformSpawner`

### What was done
Created `Scripts/Platforms/DifficultyManager.cs` and moved these methods out of `PlatformSpawner`:
- `GetDifficulty01()` → `GetDifficulty()`
- `ComputeRowStep()` → `ComputeRowStep()`
- `ComputePlatformCount()` → `ComputePlatformCount()`
- `GetMovingChance()` / `GetSpikesChance()` — new helper methods
- All difficulty-related `[SerializeField]` fields (easyRowFraction, hardRowFraction, etc.)

`PlatformSpawner` now holds a reference to `DifficultyManager` and calls its API.

### Why this is better
**Single Responsibility Principle**: The old `PlatformSpawner` was 351 lines handling spawning, difficulty curves, X-position generation, heart spawning, and despawning. Now:
- `DifficultyManager` — only computes difficulty values (~90 lines)
- `PlatformSpawner` — only spawns and despawns objects

This makes each class easier to read, test, and modify independently.

---

## Step 10: Optimize `PlayerPrefs.Save()` (Batched Saves)

### What was done
- **`PlayerHealth`**: Removed `SaveHealth()` calls from `TakeDamage()` and `Heal()`. Made `SaveHealth()` public. It's now called only by `GameOverController` on game-over.
- **`ScoreManager`**: Removed `PlayerPrefs.Save()` from the per-frame `Update()`. Added `SaveBestScore()` method, called by `GameOverController` on game-over.
- **`GameOverController`**: Now calls `playerHealth.SaveHealth()` and `scoreManager.SaveBestScore()` in `TriggerGameOver()`.

### Why this is better
`PlayerPrefs.Save()` performs a synchronous file write. On mobile:
- **Before**: Called every frame (score) + every damage event (health) = 60+ file writes/second
- **After**: Called once at game-over = 1 file write per session

This eliminates frame hitches caused by disk I/O on Android/iOS.

---

## Step 11: Remove `GameObject.Find` / `FindObjectOfType` Fallbacks

### What was done
- **`MainMenuController`**: Removed `Awake()` that called `GameObject.Find("SettingsPanel")` and `GameObject.Find("CreditsPanel")`.

### Why this is better
`GameObject.Find()` searches the entire scene hierarchy by string name — it's slow, fragile (breaks if the name changes), and hides missing Inspector assignments. Requiring explicit Inspector assignment is:
- Faster (zero runtime cost)
- Safer (missing reference → visible error in editor)
- Self-documenting (Inspector shows all dependencies)

---

## Step 12: Add Kazakh Word Collection System

### What was done
Created 4 new scripts in `Scripts/Words/`:

| Script | Purpose |
|---|---|
| `KazakhWord.cs` | Data class: holds kazakh text, english translation, and category |
| `KazakhWordBank.cs` | ScriptableObject: stores all words, picks random ones at runtime |
| `WordCollectable.cs` | Floats above platforms, displays word, triggers collection on touch |
| `WordCollector.cs` | On the Player: tracks collected words, fires events, detects combos |
| `WordUI.cs` | Displays word count, last collected word, and combo notifications |

### How the automatic word system works
1. `KazakhWordBank` is a ScriptableObject with 35 built-in Kazakh words across 3 categories (Nature, Family, Values).
2. `PlatformSpawner` automatically spawns `WordCollectable` prefabs above safe platforms with a configurable chance (default 35%).
3. Each spawned word calls `wordBank.GetRandomWord()` to pick a random entry — **no manual placement needed**.
4. When the player touches a word, `WordCollector` adds it to their collection and fires events.
5. `WordUI` subscribes to these events and displays the word + translation popup.
6. Collecting 3+ words of the same category triggers a **combo** event.

### Why this approach makes the game smooth and endless
- **Zero manual work**: Words generate from a dictionary, not hand-placed in the scene.
- **Infinite variety**: 35 words × random selection = the player sees different words each run.
- **Easy to expand**: Add more words by editing the ScriptableObject asset — no code changes.
- **Category combos**: Give the collection mechanic depth without quizzes or pressure.
- **Data-driven**: Designers can create multiple `KazakhWordBank` assets for different difficulty levels or themes.

---

## Step 13: Add `isDead` Guard to `PlayerHealth`

### What was done
Added `private bool isDead` field. `TakeDamage()` now returns early if the player is already dead.

### Why this is better
Previously, if multiple spikes hit the player on the same frame (after i-frames expire), `OnDied` could fire multiple times, causing `GameOverController.TriggerGameOver()` to run twice — potentially double-freezing time or corrupting UI state. The guard ensures death fires exactly once.

---

## Summary of Architecture Improvements

| Before | After |
|---|---|
| 5 file/class name mismatches | All files match their class names |
| `[UnityEngine.SerializeField]` everywhere | Clean `[SerializeField]` |
| Russian comments | English comments |
| `Debug.Log` in production | Removed or wrapped in `#if UNITY_EDITOR` |
| No damage interface | `IDamageable` decouples damage system |
| 351-line PlatformSpawner | Split into `PlatformSpawner` + `DifficultyManager` |
| No word system | Full Kazakh word collection with combos |
| `PlayerPrefs.Save()` every frame | Batched saves at game-over only |
| No input validation | `maxHealth`, `dragSensitivity`, gravity checks |
| `GameObject.Find` fallbacks | Inspector-only assignment |
