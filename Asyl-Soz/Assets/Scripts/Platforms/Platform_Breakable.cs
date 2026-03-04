using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BreakablePlatform : MonoBehaviour
{
    [UnityEngine.SerializeField] private float breakDelay = 0.15f;
    [UnityEngine.SerializeField] private float fallGravity = 4f;

    private Rigidbody2D rb;
    private bool broken;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (broken) return;
        if (!collision.collider.CompareTag("Player")) return;

        ContactPoint2D contact = collision.GetContact(0);
        if (contact.normal.y < 0.5f) return; // only from above

        broken = true;
        StartCoroutine(BreakRoutine());
    }

    private IEnumerator BreakRoutine()
    {
        yield return new WaitForSeconds(breakDelay);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;

        Destroy(gameObject, 2f);
    }
}