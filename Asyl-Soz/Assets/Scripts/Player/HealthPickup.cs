using UnityEngine;

public class HealthPickup2D : MonoBehaviour
{
    [UnityEngine.SerializeField] private int healAmount = 1;
    [UnityEngine.SerializeField] private bool rotate = true;
    [UnityEngine.SerializeField] private float rotateSpeed = 90f;

    private void Update()
    {
        if (rotate)
            transform.Rotate(0f, 0f, rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var hp = other.GetComponent<PlayerHealth>();
        if (hp != null)
            hp.Heal(healAmount);

        Destroy(gameObject);
    }
}