using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerJumpController : MonoBehaviour
{
    [Header("Jump")]
    [UnityEngine.SerializeField] private float jumpForce = 12f;

    [Header("Landing Check")]
    [UnityEngine.SerializeField] private bool requireTopLanding = true;

    [Range(0f, 1f)]
    [UnityEngine.SerializeField] private float minUpNormal = 0.5f;

    [Header("Wrap Around")]
    [UnityEngine.SerializeField] private bool wrapAround = true;

    [UnityEngine.SerializeField] private float wrapPadding = 0.5f;

    private Rigidbody2D rb;
    private Camera cam;

    // ✅ Это нужно PlatformSpawner для автокалибровки
    public float JumpForce => jumpForce;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
    }

    private void Update()
    {
        if (wrapAround) HandleWrapAround();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Platform")) return;

        // jump only when falling
        if (rb.linearVelocity.y > 0f) return;

        if (requireTopLanding)
        {
            var contact = collision.GetContact(0);
            if (contact.normal.y < minUpNormal) return;
        }

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void HandleWrapAround()
    {
        if (cam == null) return;

        Vector3 pos = transform.position;

        float leftX  = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, 0f)).x;
        float rightX = cam.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f)).x;

        if (pos.x < leftX - wrapPadding) pos.x = rightX + wrapPadding;
        else if (pos.x > rightX + wrapPadding) pos.x = leftX - wrapPadding;

        transform.position = pos;
    }
}