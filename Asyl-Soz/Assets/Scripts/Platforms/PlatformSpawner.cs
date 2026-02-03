using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Transform player;
    [UnityEngine.SerializeField] private Rigidbody2D playerRb;
    [UnityEngine.SerializeField] private Camera cam;
    [UnityEngine.SerializeField] private GameObject platformPrefab;

    [Tooltip("Drag the PlayerJumpController from Player here.")]
    [UnityEngine.SerializeField] private PlayerJumpController jumpController;

    [Header("Auto Distance (based on jump height)")]
    [Tooltip("Lower = platforms closer. Good: 0.55–0.70")]
    [UnityEngine.SerializeField] private float minJumpFactor = 0.60f;

    [Tooltip("Higher = platforms farther. Good: 0.80–0.95")]
    [UnityEngine.SerializeField] private float maxJumpFactor = 0.88f;

    [Tooltip("Clamp so it never becomes too tiny.")]
    [UnityEngine.SerializeField] private float minStepClamp = 0.8f;

    [Tooltip("Clamp so it never becomes impossible.")]
    [UnityEngine.SerializeField] private float maxStepClamp = 3.5f;

    [Header("Spawn Flow")]
    [UnityEngine.SerializeField] private int initialPlatforms = 18;
    [UnityEngine.SerializeField] private float spawnAhead = 14f;
    [UnityEngine.SerializeField] private float despawnBelow = 12f;

    [Header("X Range (camera width)")]
    [UnityEngine.SerializeField] private float xPadding = 0.8f;

    // These will be auto-calibrated at runtime:
    private float minYStep = 1.2f;
    private float maxYStep = 2.0f;

    private float highestY;
    private readonly List<GameObject> spawned = new();

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (playerRb == null && player != null)
            playerRb = player.GetComponent<Rigidbody2D>();

        if (jumpController == null && player != null)
            jumpController = player.GetComponent<PlayerJumpController>();
    }

    private void Start()
    {
        if (platformPrefab == null || cam == null || player == null || playerRb == null || jumpController == null)
        {
            Debug.LogError("PlatformSpawner: Missing references. Assign Player, PlayerRb, JumpController, Camera, and PlatformPrefab.");
            return;
        }

        CalibrateStepsFromJump();

        // Start platform under player
        float startY = player.position.y - 1.5f;
        SpawnPlatform(new Vector2(0f, startY));
        highestY = startY;

        for (int i = 0; i < initialPlatforms; i++)
            SpawnNext();
    }

    private void Update()
    {
        if (platformPrefab == null || cam == null || player == null) return;

        while (highestY < player.position.y + spawnAhead)
            SpawnNext();

        float camY = cam.transform.position.y;

        for (int i = spawned.Count - 1; i >= 0; i--)
        {
            if (spawned[i] == null)
            {
                spawned.RemoveAt(i);
                continue;
            }

            if (spawned[i].transform.position.y < camY - despawnBelow)
            {
                Destroy(spawned[i]);
                spawned.RemoveAt(i);
            }
        }
    }

    private void CalibrateStepsFromJump()
    {
        // gravity magnitude in world units/sec^2
        float g = Mathf.Abs(Physics2D.gravity.y * playerRb.gravityScale);
        if (g < 0.0001f) g = 9.81f;

        // In our jump code, we set rb.velocity.y = jumpForce, so treat jumpForce as initial jump velocity
        float v0 = Mathf.Abs(jumpController.JumpForce);

        // Max height formula: h = v^2 / (2g)
        float maxJumpHeight = (v0 * v0) / (2f * g);

        // Convert to platform spacing range
        float minStep = maxJumpHeight * minJumpFactor;
        float maxStep = maxJumpHeight * maxJumpFactor;

        // Safety clamps
        minYStep = Mathf.Clamp(minStep, minStepClamp, maxStepClamp);
        maxYStep = Mathf.Clamp(maxStep, minYStep + 0.1f, maxStepClamp);

        Debug.Log($"[PlatformSpawner] Calibrated Y steps: {minYStep:F2} .. {maxYStep:F2} (maxJumpHeight ~ {maxJumpHeight:F2})");
    }

    private void SpawnNext()
    {
        float yStep = Random.Range(minYStep, maxYStep);
        float newY = highestY + yStep;

        // Reliable bounds for Orthographic camera
        float camX = cam.transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;

        float minX = camX - halfWidth + xPadding;
        float maxX = camX + halfWidth - xPadding;

        // If padding too large for current camera width, fallback
        if (minX > maxX)
        {
            minX = camX - halfWidth;
            maxX = camX + halfWidth;
        }

        float x = Random.Range(minX, maxX);

        SpawnPlatform(new Vector2(x, newY));
        highestY = newY;
    }

    private void SpawnPlatform(Vector2 pos)
    {
        var go = Instantiate(platformPrefab, pos, Quaternion.identity, transform);
        spawned.Add(go);
    }
}