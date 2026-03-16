using UnityEngine;
using TMPro;

/// <summary>
/// Displays word collection count and the last collected word with its translation.
/// Subscribes to WordCollector events — no polling, no tight coupling.
/// </summary>
public class WordUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WordCollector collector;

    [Header("UI Elements")]
    [SerializeField] private TMP_Text wordCountText;
    [SerializeField] private TMP_Text lastWordText;
    [SerializeField] private TMP_Text comboText;

    [Header("Settings")]
    [Tooltip("How long the last-word popup stays visible (seconds).")]
    [SerializeField] private float popupDuration = 2f;

    private float popupTimer;
    private float comboTimer;

    private void OnEnable()
    {
        if (collector != null)
        {
            collector.OnWordCollected += OnWordCollected;
            collector.OnCombo += OnCombo;
        }
    }

    private void OnDisable()
    {
        if (collector != null)
        {
            collector.OnWordCollected -= OnWordCollected;
            collector.OnCombo -= OnCombo;
        }
    }

    private void Start()
    {
        UpdateCountUI(0);
        if (lastWordText != null) lastWordText.gameObject.SetActive(false);
        if (comboText != null) comboText.gameObject.SetActive(false);
    }

    private void Update()
    {
        // Auto-hide the last-word popup
        if (popupTimer > 0f)
        {
            popupTimer -= Time.unscaledDeltaTime;
            if (popupTimer <= 0f && lastWordText != null)
                lastWordText.gameObject.SetActive(false);
        }

        // Auto-hide combo text
        if (comboTimer > 0f)
        {
            comboTimer -= Time.unscaledDeltaTime;
            if (comboTimer <= 0f && comboText != null)
                comboText.gameObject.SetActive(false);
        }
    }

    private void OnWordCollected(KazakhWord word, int total)
    {
        UpdateCountUI(total);

        if (lastWordText != null)
        {
            lastWordText.text = $"{word.kazakh} — {word.english}";
            lastWordText.gameObject.SetActive(true);
            popupTimer = popupDuration;
        }
    }

    private void OnCombo(WordCategory category, int streak)
    {
        if (comboText != null)
        {
            comboText.text = $"{category} combo x{streak}!";
            comboText.gameObject.SetActive(true);
            comboTimer = popupDuration;
        }
    }

    private void UpdateCountUI(int count)
    {
        if (wordCountText != null)
            wordCountText.text = $"Words: {count}";
    }
}
