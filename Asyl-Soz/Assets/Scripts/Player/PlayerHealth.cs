using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    public event Action<int, int> OnHealthChanged; // current, max
    public event Action OnDied;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private int startHealth = 3;

    [Header("I-Frames")]
    [SerializeField] private float invulnerableTime = 0.6f;

    [Header("Save")]
    [SerializeField] private bool saveToPlayerPrefs = true;

    private int currentHealth;
    private float invulnTimer;
    private bool isDead;

    private const string HealthKey = "CURRENT_HEALTH";

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        ValidateConfig();
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
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        invulnTimer = invulnerableTime;

        Notify();

        if (currentHealth <= 0)
        {
            isDead = true;
            OnDied?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        Notify();
    }

    public void ResetToStart()
    {
        isDead = false;
        currentHealth = Mathf.Clamp(startHealth, 1, maxHealth);
        Notify();
    }

    /// <summary>
    /// Saves current HP to PlayerPrefs. Call on game-over or scene transitions only —
    /// avoid calling every frame to prevent file-write hitches on mobile.
    /// </summary>
    public void SaveHealth()
    {
        if (!saveToPlayerPrefs) return;
        PlayerPrefs.SetInt(HealthKey, currentHealth);
        PlayerPrefs.Save();
    }

    private void Notify()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void ValidateConfig()
    {
        if (maxHealth <= 0)
        {
            Debug.LogError("PlayerHealth: maxHealth must be > 0. Defaulting to 3.");
            maxHealth = 3;
        }

        startHealth = Mathf.Clamp(startHealth, 1, maxHealth);
    }

    private void LoadHealth()
    {
        if (saveToPlayerPrefs)
        {
            int saved = PlayerPrefs.GetInt(HealthKey, -1);
            if (saved >= 0)
            {
                // If saved value is 0, start with at least 1 HP
                currentHealth = Mathf.Clamp(saved, 1, maxHealth);
                return;
            }
        }

        currentHealth = Mathf.Clamp(startHealth, 1, maxHealth);
    }

    /// <summary>
    /// Call before Restart so we don't load stale or zero HP on the next scene.
    /// </summary>
    public static void ClearSavedHealth()
    {
        PlayerPrefs.DeleteKey(HealthKey);
        PlayerPrefs.Save();
    }
}