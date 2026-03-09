using System.Collections.Generic;
using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerJumpController jumpController;

    [Header("Platform Prefabs")]
    [SerializeField] private GameObject normalPlatformPrefab;
    [SerializeField] private GameObject movingPlatformPrefab;
    [SerializeField] private GameObject spikesPlatformPrefab;

    [Header("Health Pickup")]
    [SerializeField] private GameObject healthPickupPrefab;

    [Header("Platform Type Chances (base)")]
    [Range(0f, 1f)] [SerializeField] private float movingChance = 0.12f;
    [Range(0f, 1f)] [SerializeField] private float spikesChance = 0.06f;

    [Header("Row Spawning")]
    [Tooltip("Platform count per row: x = minimum at max difficulty, y = maximum at start.")]
    [SerializeField] private Vector2Int platformsPerRow = new Vector2Int(1, 3);

    [Tooltip("Minimum horizontal gap between platforms in the same row (world units).")]
    [SerializeField] private float minXSeparation = 1.8f;

    [Header("Safe Path Rules")]
    [Tooltip("Each row always contains at least one non-spike platform.")]
    [SerializeField] private bool guaranteeSafePlatformEachRow = true;

    [Tooltip("Spike platforms are never the only platform in a row.")]
    [SerializeField] private bool spikesNeverAlone = true;

    [Header("Heart Pickup")]
    [Range(0f, 1f)] [SerializeField] private float heartChance = 0.10f;
    [SerializeField] private float heartYOffset = 0.65f;

    [Header("Difficulty Curve")]
    [Tooltip("Row gap as a fraction of max jump height at the easiest point.")]
    [Range(0.2f, 0.6f)] [SerializeField] private float easyRowFraction = 0.40f;

    [Tooltip("Row gap as a fraction of max jump height at full difficulty.")]
    [Range(0.6f, 0.92f)] [SerializeField] private float hardRowFraction = 0.82f;

    [Tooltip("Height (world units) at which full difficulty is reached.")]
    [SerializeField] private float difficultyRampHeight = 300f;

    [Tooltip("Height (world units) with no hazards or moving platforms — lets the player learn the jump.")]
    [SerializeField] private float gracePeriodHeight = 35f;

    [Header("Spawn Settings")]
    [SerializeField] private int initialRows = 12;
    [SerializeField] private float spawnAhead = 16f;
    [SerializeField] private float despawnBelow = 14f;
    [SerializeField] private float xPadding = 0.5f;

    // Heart chance multiplier for secondary (non-guaranteed-safe) platforms.
    private const float SecondaryHeartChanceMultiplier = 0.35f;
    // Safety cap: row step never exceeds this fraction of max jump height.
    private const float MaxSafeRowFraction = 0.88f;

    private float maxJumpHeight;
    private float highestRowY;

    private readonly List<GameObject> spawnedPlatforms = new();

    // ── Lifecycle ────────────────────────────────────────────────────────────

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        if (!ValidateReferences()) return;

        CalibrateJumpHeight();
        SpawnInitialPlatforms();
    }

    private void Update()
    {
        if (player == null || cam == null) return;

        SpawnAheadOfPlayer();
        DespawnBehindCamera();
    }

    // ── Initialization ───────────────────────────────────────────────────────

    private void ResolveReferences()
    {
        if (cam == null)
            cam = Camera.main;

        if (player == null)
        {
            var playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) player = playerObject.transform;
        }

        if (player != null)
        {
            if (playerRb == null) playerRb = player.GetComponent<Rigidbody2D>();
            if (jumpController == null) jumpController = player.GetComponent<PlayerJumpController>();
        }
    }

    private bool ValidateReferences()
    {
        if (cam == null || player == null || playerRb == null || jumpController == null)
        {
            Debug.LogError("PlatformSpawner: Missing required references (Camera / Player / Rigidbody2D / JumpController).");
            return false;
        }

        if (normalPlatformPrefab == null)
        {
            Debug.LogError("PlatformSpawner: normalPlatformPrefab is not assigned.");
            return false;
        }

        return true;
    }

    private void CalibrateJumpHeight()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * playerRb.gravityScale);
        if (gravity < 0.0001f) gravity = 9.81f;

        float jumpVelocity = Mathf.Abs(jumpController.JumpForce);
        maxJumpHeight = (jumpVelocity * jumpVelocity) / (2f * gravity);
    }

    private void SpawnInitialPlatforms()
    {
        float startY = player.position.y - 1.4f;
        SpawnSafePlatformAt(new Vector2(0f, startY));
        highestRowY = startY;

        for (int i = 0; i < initialRows; i++)
            SpawnNextRow();
    }

    // ── Spawn Loop ───────────────────────────────────────────────────────────

    private void SpawnAheadOfPlayer()
    {
        while (highestRowY < player.position.y + spawnAhead)
            SpawnNextRow();
    }

    private void DespawnBehindCamera()
    {
        float cameraBottomY = cam.transform.position.y - despawnBelow;

        for (int i = spawnedPlatforms.Count - 1; i >= 0; i--)
        {
            if (spawnedPlatforms[i] == null)
            {
                spawnedPlatforms.RemoveAt(i);
                continue;
            }

            if (spawnedPlatforms[i].transform.position.y < cameraBottomY)
            {
                Destroy(spawnedPlatforms[i]);
                spawnedPlatforms.RemoveAt(i);
            }
        }
    }

    private void SpawnNextRow()
    {
        float difficulty = GetDifficulty01(highestRowY);
        float rowY = highestRowY + ComputeRowStep(difficulty);
        bool inGracePeriod = rowY < gracePeriodHeight;

        // At difficulty=1: moving up to +22%, spikes up to +18%. Both suppressed during grace period.
        float movingNow = inGracePeriod ? 0f : Mathf.Lerp(movingChance, movingChance + 0.22f, difficulty);
        float spikesNow = inGracePeriod ? 0f : Mathf.Lerp(spikesChance, spikesChance + 0.18f, difficulty);

        int platformCount = ComputePlatformCount(difficulty);
        (float minX, float maxX) = GetCameraXBounds();
        List<float> xPositions = PickRowXPositions(platformCount, minX, maxX);

        if (guaranteeSafePlatformEachRow)
            SpawnRowWithGuaranteedSafe(xPositions, rowY, spikesNow, movingNow, difficulty);
        else
            SpawnRowFree(xPositions, rowY, spikesNow, movingNow);

        highestRowY = rowY;
    }

    // ── Difficulty Helpers ───────────────────────────────────────────────────

    private float GetDifficulty01(float worldY) =>
        Mathf.Clamp01(Mathf.Max(0f, worldY) / difficultyRampHeight);

    /// <summary>
    /// Row step scales from ~40% to ~82% of max jump height as difficulty rises.
    /// Both the min and max of the random range increase, so gaps grow but remain reachable.
    /// </summary>
    private float ComputeRowStep(float difficulty)
    {
        float minFraction = Mathf.Lerp(easyRowFraction,        hardRowFraction * 0.88f, difficulty);
        float maxFraction = Mathf.Lerp(easyRowFraction * 1.35f, hardRowFraction,        difficulty);

        float step = maxJumpHeight * Random.Range(minFraction, maxFraction);

        // Hard cap: never exceed 88% of jump height so the next row is always reachable.
        return Mathf.Clamp(step, 0.7f, maxJumpHeight * MaxSafeRowFraction);
    }

    /// <summary>
    /// Platform count fades from platformsPerRow.y (easy) down to platformsPerRow.x (hard).
    /// </summary>
    private int ComputePlatformCount(float difficulty)
    {
        int maxCount = Mathf.RoundToInt(Mathf.Lerp(platformsPerRow.y, platformsPerRow.x + 0.5f, difficulty));
        int minCount = Mathf.Max(1, maxCount - 1);
        return Random.Range(minCount, maxCount + 1);
    }

    private void SpawnRowWithGuaranteedSafe(
        List<float> xs, float rowY,
        float spikesChanceNow, float movingChanceNow, float difficulty)
    {
        int safeIndex = Random.Range(0, xs.Count);

        for (int i = 0; i < xs.Count; i++)
        {
            Vector2 pos = new Vector2(xs[i], rowY);

            if (i == safeIndex)
            {
                SpawnSafePlatformAt(pos, movingChanceNow);
                TrySpawnHeartAbove(pos, difficulty);
            }
            else
            {
                bool spawnSpike = spikesPlatformPrefab != null
                    && !(spikesNeverAlone && xs.Count == 1)
                    && Random.value < spikesChanceNow;

                SpawnPlatform(spawnSpike ? spikesPlatformPrefab : ChooseSafePrefab(movingChanceNow), pos);

                if (!spawnSpike)
                    TrySpawnHeartAbove(pos, difficulty, SecondaryHeartChanceMultiplier);
            }
        }
    }

    private void SpawnRowFree(
        List<float> xs, float rowY,
        float spikesChanceNow, float movingChanceNow)
    {
        bool spawnedAnySafe = false;

        foreach (float x in xs)
        {
            Vector2 pos = new Vector2(x, rowY);
            bool spawnSpike = spikesPlatformPrefab != null && Random.value < spikesChanceNow;

            SpawnPlatform(spawnSpike ? spikesPlatformPrefab : ChooseSafePrefab(movingChanceNow), pos);

            if (!spawnSpike) spawnedAnySafe = true;
        }

        // Fallback: if every slot rolled as spikes, add a safe platform at the first position.
        if (spikesNeverAlone && !spawnedAnySafe)
            SpawnSafePlatformAt(new Vector2(xs[0], rowY), movingChanceNow);
    }

    // ── Platform Helpers ─────────────────────────────────────────────────────

    private (float min, float max) GetCameraXBounds()
    {
        float camX = cam.transform.position.x;
        float halfWidth = cam.orthographicSize * cam.aspect;
        return (camX - halfWidth + xPadding, camX + halfWidth - xPadding);
    }

    private List<float> PickRowXPositions(int count, float minX, float maxX)
    {
        var xs = new List<float>(count);
        if (count <= 0) return xs;

        xs.Add(Random.Range(minX, maxX));

        int attempts = 0;
        while (xs.Count < count && attempts < 200)
        {
            attempts++;
            float candidate = Random.Range(minX, maxX);

            bool tooClose = false;
            foreach (float x in xs)
            {
                if (Mathf.Abs(candidate - x) < minXSeparation)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose) xs.Add(candidate);
        }

        // Fallback: screen is too narrow to fit all platforms with full separation.
        while (xs.Count < count)
            xs.Add(Random.Range(minX, maxX));

        return xs;
    }

    private void SpawnSafePlatformAt(Vector2 pos, float movingChanceNow = 0f)
    {
        SpawnPlatform(ChooseSafePrefab(movingChanceNow), pos);
    }

    private GameObject ChooseSafePrefab(float movingChanceNow)
    {
        if (movingPlatformPrefab != null && Random.value < movingChanceNow)
            return movingPlatformPrefab;

        return normalPlatformPrefab;
    }

    private void SpawnPlatform(GameObject prefab, Vector2 pos)
    {
        if (prefab == null) return;
        var go = Instantiate(prefab, pos, Quaternion.identity, transform);
        spawnedPlatforms.Add(go);
    }

    private void TrySpawnHeartAbove(Vector2 platformPos, float difficulty, float chanceMultiplier = 1f)
    {
        if (healthPickupPrefab == null) return;

        float chance = Mathf.Lerp(heartChance, heartChance * 0.6f, difficulty) * chanceMultiplier;

        if (Random.value < chance)
            Instantiate(healthPickupPrefab, platformPos + Vector2.up * heartYOffset, Quaternion.identity, transform);
    }
}