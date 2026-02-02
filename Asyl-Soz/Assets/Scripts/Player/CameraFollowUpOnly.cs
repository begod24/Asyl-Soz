using UnityEngine;

/// <summary>
/// Follows a target only when it goes higher than the current camera height + offset.
/// Camera never moves downward (classic Doodle Jump behavior).
/// </summary>
public class CameraFollowUpOnly : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow")]
    [Tooltip("How far above the target the camera should be (usually 0..2).")]
    [SerializeField] private float yOffset = 1.0f;

    [Tooltip("How smooth the camera movement is. Higher = faster catch-up.")]
    [SerializeField] private float smoothSpeed = 8.0f;

    [Header("Optional Limits")]
    [Tooltip("Minimum camera Y (useful if you start below 0).")]
    [SerializeField] private float minY = -9999f;

    private float _highestY; // current locked camera Y (never decreases)

    private void Awake()
    {
        // Initialize the camera's starting "highest" Y
        _highestY = transform.position.y;

        // If target not set, try find by tag
        if (target == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) target = go.transform;
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        float desiredY = target.position.y + yOffset;

        // Only follow upward
        if (desiredY > _highestY)
            _highestY = desiredY;

        // Apply optional minimum clamp
        float clampedY = Mathf.Max(_highestY, minY);

        // Smooth movement towards clampedY (never down because _highestY never decreases)
        Vector3 currentPos = transform.position;
        Vector3 targetPos = new Vector3(currentPos.x, clampedY, currentPos.z);

        float t = 1f - Mathf.Exp(-smoothSpeed * Time.deltaTime); // smooth regardless of FPS
        transform.position = Vector3.Lerp(currentPos, targetPos, t);
    }

    /// <summary>Call this if you restart without reloading the scene.</summary>
    public void ResetLockToCurrentPosition()
    {
        _highestY = transform.position.y;
    }
}