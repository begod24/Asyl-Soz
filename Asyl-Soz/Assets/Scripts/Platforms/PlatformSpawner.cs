using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Transform player;
    [UnityEngine.SerializeField] private Rigidbody2D playerRb;
    [UnityEngine.SerializeField] private Camera cam;
    [UnityEngine.SerializeField] private PlayerJumpController jumpController;

    [Header("Platform Prefabs")]
    [UnityEngine.SerializeField] private GameObject normalPlatformPrefab;
    [UnityEngine.SerializeField] private GameObject movingPlatformPrefab;
    [UnityEngine.SerializeField] private GameObject spikesPlatformPrefab;

    [Header("Pickup Prefabs")]
    [Tooltip("Optional. If assigned, hearts can spawn occasionally.")]
    [UnityEngine.SerializeField] private GameObject healthPickupPrefab;

    [Header("Chances")]
    [Range(0f, 1f)] [UnityEngine.SerializeField] private float movingChance = 0.20f;
    [Range(0f, 1f)] [UnityEngine.SerializeField] private float spikesChance = 0.12f;

    [Header("Heart Spawn")]
    [Range(0f, 1f)] [UnityEngine.SerializeField] private float heartChance = 0.10f;
    [UnityEngine.SerializeField] private float heartYOffset = 0.6f;

    [Header("Auto Distance (based on jump height)")]
    [UnityEngine.SerializeField] private float minJumpFactor = 0.60f;
    [UnityEngine.SerializeField] private float maxJumpFactor = 0.88f;
    [UnityEngine.SerializeField] private float minStepClamp = 1.0f;
    [UnityEngine.SerializeField] private float maxStepClamp = 3.6f;

    [Header("Spawn Flow")]
    [UnityEngine.SerializeField] private int initialPlatforms = 18;
    [UnityEngine.SerializeField] private float spawnAhead = 14f;
    [UnityEngine.SerializeField] private float despawnBelow = 12f;

    [Header("X Range")]
    [UnityEngine.SerializeField] private float xPadding = 0.8f;

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

        if (player != null)
        {
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
            if (jumpController == null) jumpController = player.GetComponent<PlayerJumpController>();
        }
    }

    private void Start()
    {
        if (cam == null || player == null || playerRb == null || jumpController == null)
        {
            Debug.LogError("PlatformSpawner: Missing Player / PlayerRb / JumpController / Camera.");
            return;
        }

        if (normalPlatformPrefab == null)
        {
            Debug.LogError("PlatformSpawner: normalPlatformPrefab is not assigned.");
            return;
        }

        CalibrateStepsFromJump();

        float startY = player.position.y - 1.5f;
        SpawnPlatform(normalPlatformPrefab, new Vector2(0f, startY));
        highestY = startY;

        for (int i = 0; i < initialPlatforms; i++)
            SpawnNext();
    }

    private void Update()
    {
        if (player == null || cam == null) return;

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
        float g = Mathf.Abs(Physics2D.gravity.y * playerRb.gravityScale);
        if (g < 0.0001f) g = 9.81f;

        float v0 = Mathf.Abs(jumpController.JumpForce);
        float maxJumpHeight = (v0 * v0) / (2f * g);

        float minStep = maxJumpHeight * minJumpFactor;
        float maxStep = maxJumpHeight * maxJumpFactor;

        minYStep = Mathf.Clamp(minStep, minStepClamp, maxStepClamp);
        maxYStep = Mathf.Clamp(maxStep, minYStep + 0.1f, maxStepClamp);
    }

    private void SpawnNext()
    {
        float yStep = Random.Range(minYStep, maxYStep);
        float newY = highestY + yStep;

        float camX = cam.transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;

        float minX = camX - halfWidth + xPadding;
        float maxX = camX + halfWidth - xPadding;
        if (minX > maxX)
        {
            minX = camX - halfWidth;
            maxX = camX + halfWidth;
        }

        float x = Random.Range(minX, maxX);
        Vector2 platformPos = new Vector2(x, newY);

        GameObject platformPrefab = ChoosePlatformPrefab();
        GameObject platformGO = SpawnPlatform(platformPrefab, platformPos);

        // Optional heart spawn above platform
        if (healthPickupPrefab != null && Random.value < heartChance)
        {
            Vector2 heartPos = platformPos + Vector2.up * heartYOffset;
            Instantiate(healthPickupPrefab, heartPos, Quaternion.identity, transform);
        }

        highestY = newY;
    }

    private GameObject ChoosePlatformPrefab()
    {
        float r = Random.value;

        if (spikesPlatformPrefab != null && r < spikesChance)
            return spikesPlatformPrefab;

        r -= spikesChance;

        if (movingPlatformPrefab != null && r < movingChance)
            return movingPlatformPrefab;

        return normalPlatformPrefab;
    }

    private GameObject SpawnPlatform(GameObject prefab, Vector2 pos)
    {
        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        spawned.Add(go);
        return go;
    }
}