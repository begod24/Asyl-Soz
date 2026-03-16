using UnityEngine;

/// <summary>
/// ScriptableObject that holds the full dictionary of Kazakh words.
/// Create via Assets → Create → Asyl Söz → Kazakh Word Bank.
///
/// WHY ScriptableObject:
/// • Separates data from logic — designers edit words without touching code.
/// • Shared across scenes, prefabs, and multiple components (no duplication).
/// • Automatically serialized by Unity — works with version control.
///
/// The bank ships with a built-in default list so the game is playable
/// out of the box, and words are selected randomly at runtime, making
/// the endless runner feel fresh without manually placing every word.
/// </summary>
[CreateAssetMenu(fileName = "KazakhWordBank", menuName = "Asyl Söz/Kazakh Word Bank")]
public class KazakhWordBank : ScriptableObject
{
    [Tooltip("All available Kazakh words. The system picks randomly at runtime.")]
    [SerializeField] private KazakhWord[] words;

    /// <summary>Returns a random word from the bank.</summary>
    public KazakhWord GetRandomWord()
    {
        if (words == null || words.Length == 0) return null;
        return words[Random.Range(0, words.Length)];
    }

    /// <summary>Returns a random word from the specified category.</summary>
    public KazakhWord GetRandomWordByCategory(WordCategory category)
    {
        if (words == null || words.Length == 0) return null;

        // Collect matching words (small array, no allocation concern at spawn time)
        int count = 0;
        foreach (var w in words)
            if (w.category == category) count++;

        if (count == 0) return GetRandomWord();

        int target = Random.Range(0, count);
        int index = 0;
        foreach (var w in words)
        {
            if (w.category != category) continue;
            if (index == target) return w;
            index++;
        }

        return GetRandomWord();
    }

    public int WordCount => words != null ? words.Length : 0;

    /// <summary>
    /// Populates the bank with a hardcoded default list of Kazakh words.
    /// Called from editor scripts or at runtime if the bank is empty.
    /// This makes the game immediately playable with real Kazakh vocabulary.
    /// </summary>
    public void PopulateDefaults()
    {
        words = new KazakhWord[]
        {
            // ── Nature (Табиғат) ──
            W("Күн",     "Sun",       WordCategory.Nature),
            W("Ай",      "Moon",      WordCategory.Nature),
            W("Жұлдыз",  "Star",      WordCategory.Nature),
            W("Су",      "Water",     WordCategory.Nature),
            W("Тау",     "Mountain",  WordCategory.Nature),
            W("Орман",   "Forest",    WordCategory.Nature),
            W("Гүл",     "Flower",    WordCategory.Nature),
            W("Жел",     "Wind",      WordCategory.Nature),
            W("Жаңбыр",  "Rain",      WordCategory.Nature),
            W("Қар",     "Snow",      WordCategory.Nature),
            W("Аспан",   "Sky",       WordCategory.Nature),
            W("Жер",     "Earth",     WordCategory.Nature),
            W("Өзен",    "River",     WordCategory.Nature),
            W("Көл",     "Lake",      WordCategory.Nature),
            W("Дала",    "Steppe",    WordCategory.Nature),

            // ── Family (Отбасы) ──
            W("Ана",     "Mother",    WordCategory.Family),
            W("Әке",     "Father",    WordCategory.Family),
            W("Бала",    "Child",     WordCategory.Family),
            W("Аға",     "Brother",   WordCategory.Family),
            W("Апа",     "Sister",    WordCategory.Family),
            W("Әже",     "Grandmother", WordCategory.Family),
            W("Ата",     "Grandfather", WordCategory.Family),
            W("Отбасы",  "Family",    WordCategory.Family),
            W("Дос",     "Friend",    WordCategory.Family),
            W("Жан",     "Soul",      WordCategory.Family),

            // ── Values (Құндылықтар) ──
            W("Махаббат", "Love",      WordCategory.Values),
            W("Бейбітшілік", "Peace", WordCategory.Values),
            W("Адалдық",  "Honesty",   WordCategory.Values),
            W("Батылдық", "Courage",   WordCategory.Values),
            W("Білім",   "Knowledge", WordCategory.Values),
            W("Денсаулық","Health",    WordCategory.Values),
            W("Бақыт",   "Happiness", WordCategory.Values),
            W("Сабырлық", "Patience",  WordCategory.Values),
            W("Құрмет",  "Respect",   WordCategory.Values),
            W("Ерлік",   "Heroism",   WordCategory.Values),
        };
    }

    private static KazakhWord W(string kz, string en, WordCategory cat)
    {
        return new KazakhWord { kazakh = kz, english = en, category = cat };
    }
}
