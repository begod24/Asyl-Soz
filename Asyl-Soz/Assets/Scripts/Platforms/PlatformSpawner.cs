using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedurally spawns platform rows ahead of the player and despawns them behind the camera.
///
/// REFACTORED:
/// - Difficulty logic moved to DifficultyManager (Single Responsibility).
/// - Kazakh words auto-spawn on safe platforms using KazakhWordBank.
/// - Magic numbers replaced with named constants or SerializeField.
/// - Gravity fallback now logs a warning so misconfiguration is visible.
/// </summary>
public class PlatformSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D playerRb;
    [SerializeField] private Camera cam;
    [SerializeField] private PlayerJumpController jumpController;
    [SerializeField] private DifficultyManager difficulty;

    [Header("Platform Prefabs")]
    [SerializeField] private GameObject normalPlatformPrefab;
    [SerializeField] private GameObject movingPlatformPrefab;
    [SerializeField] private GameObject spikesPlatformPrefab;

    [Header("Pickups")]
    [SerializeField] private GameObject healthPickupPrefab;
    [SerializeField] private GameObject wordCollectablePrefab;

    [Header("Word System")]
    [SerializeField] private KazakhWordBank wordBank;
    [Range(0f, 1f)]
    [SerializeField] private float wordSpawnChance = 0.35f;
    [SerializeField] private float wordYOffset = 0.7f;

    [Header("Heart Pickup")]
    [Range(0f, 1f)]
    [SerializeField] private float heartChance = 0.10f;
    [SerializeField] private float heartYOffset = 0.65f;

    [Header("Safe Path Rules")]
    [SerializeField] private bool guaranteeSafePlatformEachRow = true;
    [SerializeField] private bool spikesNeverAlone = true;

    [Header("Row Layout")]
    [SerializeField] private float minXSeparation = 1.8f;

    [Header("Spawn Settings")]
    [SerializeField] private int initialRows = 12;
    [SerializeField] private float spawnAhead = 16f;
    [SerializeField] private float despawnBelow = 14f;
    [SerializeField] private float xPadding = 0.5f;

    // Named constant replaces the old magic number 1.4f
    private const float StartPlatformOffset = 1.4f;
    private const float SecondaryHeartChanceMultiplier = 0.35f;

    private float maxJumpHeight;
    private float highestRowY;

    private readonly List<GameObject> spawnedObjects = new();

    // -- Lifecycle --------------------------------------------------------

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

    // -- Initialization ----------------------------------------------------

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

        if (difficulty == null)
            difficulty = GetComponent<DifficultyManager>();
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

        if (difficulty == null)
        {
            Debug.LogError("PlatformSpawner: DifficultyManager is not assigned. Add it as a sibling component.");
            return false;
        }

        return true;
    }

    private void CalibrateJumpHeight()
    {
        float gravity = Mathf.Abs(Physics2D.gravity.y * playerRb.gravityScale);
        if (gravity < 0.0001f)
        {
            Debug.LogWarning("PlatformSpawner: Gravity near zero. Using fallback 9.81. Check Rigidbody2D gravity scale.");
            gravity = 9.81f;
        }

        float jumpVelocity = Mathf.Abs(jumpController.JumpForce);
        maxJumpHeight = (jumpVelocity * jumpVelocity) / (2f * gravity);
    }

    private void SpawnInitialPlatforms()
    {
        float startY = player.position.y - StartPlatformOffset;
        SpawnSafePlatformAt(new Vector2(0f, startY));
        highestRowY = startY;

        for (int i = 0; i < initialRows; i++)
            SpawnNextRow();
    }

    // -- Spawn Loop -------------------------------------------------------

    private void SpawnAheadOfPlayer()
    {
        while (highestRowY < player.position.y + spawnAhead)
            SpawnNextRow();
    }

    private void DespawnBehindCamera()
    {
        float cameraBottomY = cam.transform.position.y - despawnBelow;

        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] == null)
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            if (spawnedObjects[i].transform.position.y < cameraBottomY)
            {
                Destroy(spawnedObjects[i]);
                spawnedObjects.RemoveAt(i);
            }
        }
    }

    private void SpawnNextRow()
    {
        float diff = difficulty.GetDifficulty(highestRowY);
        float rowY = highestRowY + difficulty.ComputeRowStep(diff, maxJumpHeight);
        bool inGracePeriod = difficulty.IsInGracePeriod(rowY);

        float movingNow = difficulty.GetMovingChance(diff, inGracePeriod);
        float spikesNow = difficulty.GetSpikesChance(diff, inGracePeriod);

        int platformCount = difficulty.ComputePlatformCount(diff);
        (float minX, float maxX) = GetCameraXBounds();
        List<float> xPositions = PickRowXPositions(platformCount, minX, maxX);

        if (guaranteeSafePlatformEachRow)
            SpawnRowWithGuaranteedSafe(xPositions, rowY, spikesNow, movingNow, diff);
        else
            SpawnRowFree(xPositions, rowY, spikesNow, movingNow, diff);

        highestRowY = rowY;
    }

    // -- Row Spawning -----------------------------------------------------

    private void SpawnRowWithGuaranteedSafe(
        List<float> xs, float rowY,
        float spikesChanceNow, float movingChanceNow, float diff)
    {
        int safeIndex = Random.Range(0, xs.Count);

        for (int i = 0; i < xs.Count; i++)
        {
            Vector2 pos = new Vector2(xs[i], rowY);

            if (i == safeIndex)
            {
                SpawnSafePlatformAt(pos, movingChanceNow);
                TrySpawnWordAbove(pos);
                TrySpawnHeartAbove(pos, diff);
            }
            else
            {
                bool spawnSpike = spikesPlatformPrefab != null
                    && !(spikesNeverAlone && xs.Count == 1)
                    && Random.value < spikesChanceNow;

                SpawnPlatform(spawnSpike ? spikesPlatformPrefab : ChooseSafePrefab(movingChanceNow), pos);

                if (!spawnSpike)
                {
                    TrySpawnWordAbove(pos);
                    TrySpawnHeartAbove(pos, diff, SecondaryHeartChanceMultiplier);
                }
            }
        }
    }

    private void SpawnRowFree(
        List<float> xs, float rowY,
        float spikesChanceNow, float movingChanceNow, float diff)
    {
        bool spawnedAnySafe = false;

        foreach (float x in xs)
        {
            Vector2 pos = new Vector2(x, rowY);
            bool spawnSpike = spikesPlatformPrefab != null && Random.value < spikesChanceNow;

            SpawnPlatform(spawnSpike ? spikesPlatformPrefab : ChooseSafePrefab(movingChanceNow), pos);

            if (!spawnSpike)
            {
                spawnedAnySafe = true;
                TrySpawnWordAbove(pos);
            }
        }

        if (spikesNeverAlone && !spawnedAnySafe)
            SpawnSafePlatformAt(new Vector2(xs[0], rowY), movingChanceNow);
    }

    // -- Platform Helpers -------------------------------------------------

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
        spawnedObjects.Add(go);
    }

    // -- Pickup Helpers ---------------------------------------------------

    private void TrySpawnHeartAbove(Vector2 platformPos, float diff, float chanceMultiplier = 1f)
    {
        if (healthPickupPrefab == null) return;

        float chance = Mathf.Lerp(heartChance, heartChance * 0.6f, diff) * chanceMultiplier;

        if (Random.value < chance)
        {
            var go = Instantiate(healthPickupPrefab, platformPos + Vector2.up * heartYOffset, Quaternion.identity, transform);
            spawnedObjects.Add(go);
        }
    }

    /// <summary>
    /// Spawns a Kazakh word collectable above the platform.
    /// Words are picked randomly from KazakhWordBank at runtime,
    /// making the game endlessly varied without manual placement.
    /// </summary>
    private void TrySpawnWordAbove(Vector2 platformPos)
    {
        if (wordCollectablePrefab == null || wordBank == null) return;
        if (Random.value > wordSpawnChance) return;

        KazakhWord word = wordBank.GetRandomWord();
        if (word == null) return;

        var go = Instantiate(wordCollectablePrefab, platformPos + Vector2.up * wordYOffset, Quaternion.identity, transform);

        var collectable = go.GetComponent<WordCollectable>();
        if (collectable != null)
            collectable.Initialize(word);

        spawnedObjects.Add(go);
    }
}
