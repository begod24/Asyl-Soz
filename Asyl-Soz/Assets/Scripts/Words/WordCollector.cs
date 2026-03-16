using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Attached to the Player. Tracks collected words, fires events for UI,
/// and handles category combos.
///
/// WHY event-driven:
/// • The collector doesn't know about UI or scoring — it just fires events.
/// • Any number of listeners (WordUI, combo display, analytics) can subscribe
///   without modifying this class (Open/Closed Principle).
/// </summary>
public class WordCollector : MonoBehaviour
{
    /// <summary>Fired when a word is collected. Passes the word and the current total.</summary>
    public event Action<KazakhWord, int> OnWordCollected;

    /// <summary>Fired when the player hits a category combo (3+ consecutive same-category words).</summary>
    public event Action<WordCategory, int> OnCombo;

    [Header("Combo")]
    [Tooltip("How many consecutive same-category words trigger a combo.")]
    [SerializeField] private int comboThreshold = 3;

    private readonly List<KazakhWord> collectedWords = new();
    private WordCategory lastCategory;
    private int consecutiveCount;

    public int TotalCollected => collectedWords.Count;
    public IReadOnlyList<KazakhWord> CollectedWords => collectedWords;

    /// <summary>Called by WordCollectable on trigger.</summary>
    public void Collect(KazakhWord word)
    {
        collectedWords.Add(word);

        // Combo tracking
        if (word.category == lastCategory)
        {
            consecutiveCount++;
        }
        else
        {
            lastCategory = word.category;
            consecutiveCount = 1;
        }

        OnWordCollected?.Invoke(word, collectedWords.Count);

        if (consecutiveCount >= comboThreshold)
            OnCombo?.Invoke(word.category, consecutiveCount);
    }

    /// <summary>Resets collector state (e.g., on restart).</summary>
    public void ResetCollection()
    {
        collectedWords.Clear();
        consecutiveCount = 0;
    }
}
