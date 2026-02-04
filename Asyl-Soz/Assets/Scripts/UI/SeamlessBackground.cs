using UnityEngine;

public class SeamlessBackgroundY : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Camera cam;
    [UnityEngine.SerializeField] private SpriteRenderer bgA;
    [UnityEngine.SerializeField] private SpriteRenderer bgB;

    [Header("Behavior")]
    [Tooltip("1 = background moves with camera (locked). < 1 = parallax (slower).")]
    [UnityEngine.SerializeField] private float parallaxFactor = 1f;

    [Tooltip("Extra overlap to hide seams (0.1–0.5).")]
    [UnityEngine.SerializeField] private float overlap = 0.2f;

    private float spriteHeightWorld;
    private float startCamY;
    private Vector3 rootStartPos;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
        rootStartPos = transform.position;
    }

    private void Start()
    {
        if (bgA == null || bgB == null || cam == null)
        {
            Debug.LogError("SeamlessBackgroundY: Assign cam, bgA, bgB.");
            enabled = false;
            return;
        }

        // Height of sprite in world units
        spriteHeightWorld = bgA.bounds.size.y;

        // Place B exactly above A with tiny overlap
        Vector3 aPos = bgA.transform.position;
        bgB.transform.position = new Vector3(aPos.x, aPos.y + spriteHeightWorld - overlap, aPos.z);

        startCamY = cam.transform.position.y;
    }

    private void LateUpdate()
    {
        if (cam == null) return;

        // Optional parallax: move the whole background root relative to camera motion
        float camDeltaY = cam.transform.position.y - startCamY;
        transform.position = rootStartPos + Vector3.up * (camDeltaY * parallaxFactor);

        // Wrap logic: if camera has moved past one sprite, move it above the other
        WrapIfNeeded(bgA, bgB);
        WrapIfNeeded(bgB, bgA);
    }

    private void WrapIfNeeded(SpriteRenderer candidate, SpriteRenderer other)
    {
        float camBottom = cam.transform.position.y - cam.orthographicSize;

        // If the candidate sprite is completely below camera bottom, move it above the other sprite
        float candidateTop = candidate.bounds.max.y;

        if (candidateTop < camBottom)
        {
            Vector3 pos = candidate.transform.position;
            pos.y = other.bounds.max.y + spriteHeightWorld - overlap;
            candidate.transform.position = pos;
        }
    }
}