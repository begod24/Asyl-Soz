using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJumpController2D : MonoBehaviour
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Platform"))
            return;

        // jump only when falling
        if (rb.linearVelocity.y > 0f)
            return;

        // ensure we landed on top
        var contact = collision.GetContact(0);
        if (contact.normal.y < 0.5f)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}
