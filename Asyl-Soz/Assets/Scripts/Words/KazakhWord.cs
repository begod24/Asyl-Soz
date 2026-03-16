using UnityEngine;

/// <summary>
/// Represents a single Kazakh word entry with its translation and category.
/// Used inside KazakhWordBank (ScriptableObject).
/// </summary>
[System.Serializable]
public class KazakhWord
{
    [Tooltip("The Kazakh word in Cyrillic or Latin script.")]
    public string kazakh;

    [Tooltip("English translation of the word.")]
    public string english;

    [Tooltip("Category for combo grouping (Nature, Family, Values, etc.).")]
    public WordCategory category;
}

public enum WordCategory
{
    Nature,
    Family,
    Values
}
