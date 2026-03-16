using UnityEngine;

public class DamageOnTouch2D : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var damageable = other.GetComponent<IDamageable>();
        if (damageable == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("DamageOnTouch2D: Player has no IDamageable component.");
#endif
            return;
        }

        damageable.TakeDamage(damage);
    }
}