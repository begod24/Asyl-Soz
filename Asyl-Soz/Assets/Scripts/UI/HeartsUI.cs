using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image[] hearts; // assign in inspector
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    [Header("Player")]
    [SerializeField] private PlayerHealth playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged += UpdateUI;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= UpdateUI;
    }

    private void Start()
    {
        if (playerHealth != null)
            UpdateUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
    }

    private void UpdateUI(int current, int max)
    {
        if (hearts == null || hearts.Length == 0) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;

            bool isFull = (i < current);
            hearts[i].sprite = isFull ? fullHeart : emptyHeart;
            hearts[i].enabled = (i < max);
        }
    }
}