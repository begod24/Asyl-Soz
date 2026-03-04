using UnityEngine;

public class DamageOnTouch2D : MonoBehaviour
{
    [UnityEngine.SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp == null)
        {
            Debug.LogWarning("DamageOnTouch2D: Player has no PlayerHealth component.");
            return;
        }

        hp.TakeDamage(damage);

        Debug.Log($"SPIKES HIT: -{damage} HP, now {hp.CurrentHealth}/{hp.MaxHealth}");
    }
}