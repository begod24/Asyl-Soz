using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    [Header("Health")]
    [UnityEngine.SerializeField] private int maxHealth = 3;
    [UnityEngine.SerializeField] private int startHealth = 3;

    [Header("I-Frames")]
    [UnityEngine.SerializeField] private float invulnerableTime = 0.6f;

    [Header("Save")]
    [UnityEngine.SerializeField] private bool saveToPlayerPrefs = true;

    private int currentHealth;
    private float invulnTimer;

    private const string HealthKey = "CURRENT_HEALTH";

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        LoadHealth();
        Notify();
    }

    private void Update()
    {
        if (invulnTimer > 0f)
            invulnTimer -= Time.unscaledDeltaTime;
    }

    public void TakeDamage(int amount)
    {
    if (amount <= 0) return;
    if (invulnTimer > 0f) return;

    currentHealth = Mathf.Max(0, currentHealth - amount);
    invulnTimer = invulnerableTime;

    SaveHealth();
    Notify();

    Debug.Log($"DAMAGE: -{amount}, HP = {currentHealth}/{maxHealth}");

    if (currentHealth <= 0)
    {
        Debug.Log("PLAYER DIED -> OnDied event fired");
        OnDied?.Invoke();
    }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        SaveHealth();
        Notify();
    }

    public void ResetToStart()
    {
        currentHealth = Mathf.Clamp(startHealth, 1, maxHealth);
        SaveHealth();
        Notify();
    }

    private void Notify()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void LoadHealth()
    {
        if (saveToPlayerPrefs)
        {
            int saved = PlayerPrefs.GetInt(HealthKey, -1);
            if (saved >= 0)
            {
                // если вдруг сохранено 0, стартуем как минимум с 1
                currentHealth = Mathf.Clamp(saved, 1, maxHealth);
                return;
            }
        }

        currentHealth = Mathf.Clamp(startHealth, 1, maxHealth);
    }

    private void SaveHealth()
    {
        if (!saveToPlayerPrefs) return;
        PlayerPrefs.SetInt(HealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    // Вызывай перед Restart, чтобы не загрузиться с 0/старым значением
    public static void ClearSavedHealth()
    {
        PlayerPrefs.DeleteKey(HealthKey);
        PlayerPrefs.Save();
    }
}