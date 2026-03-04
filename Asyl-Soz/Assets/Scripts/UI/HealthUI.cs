using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("UI")]
    [UnityEngine.SerializeField] private Image[] hearts; // assign in inspector
    [UnityEngine.SerializeField] private Sprite fullHeart;
    [UnityEngine.SerializeField] private Sprite emptyHeart;

    [Header("Player")]
    [UnityEngine.SerializeField] private PlayerHealth playerHealth;

    private void Awake()
    {
        if (playerHealth == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerHealth = p.GetComponent<PlayerHealth>();
        }
    }

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