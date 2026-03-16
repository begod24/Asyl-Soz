using UnityEngine;

/// <summary>
/// Calculates difficulty-dependent values for the platform spawner.
/// Extracted from PlatformSpawner to follow Single Responsibility Principle —
/// the spawner only spawns; this class only computes difficulty curves.
/// </summary>
public class DifficultyManager : MonoBehaviour
{
    [Header("Difficulty Curve")]
    [Tooltip("Row gap as a fraction of max jump height at the easiest point.")]
    [Range(0.2f, 0.6f)]
    [SerializeField] private float easyRowFraction = 0.40f;

    [Tooltip("Row gap as a fraction of max jump height at full difficulty.")]
    [Range(0.6f, 0.92f)]
    [SerializeField] private float hardRowFraction = 0.82f;

    [Tooltip("Height (world units) at which full difficulty is reached.")]
    [SerializeField] private float difficultyRampHeight = 300f;

    [Tooltip("Height (world units) with no hazards — lets the player learn the jump.")]
    [SerializeField] private float gracePeriodHeight = 35f;

    [Header("Platform Type Chances (base)")]
    [Range(0f, 1f)]
    [SerializeField] private float movingChance = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float spikesChance = 0.06f;

    [Header("Row Spawning")]
    [Tooltip("Platform count per row: x = minimum at max difficulty, y = maximum at start.")]
    [SerializeField] private Vector2Int platformsPerRow = new Vector2Int(1, 3);

    // Difficulty ramp amounts at max difficulty
    private const float MovingRampBonus = 0.22f;
    private const float SpikesRampBonus = 0.18f;
    private const float MaxSafeRowFraction = 0.88f;

    // ── Public API ────────────────────────────────────────────────────────

    public float GracePeriodHeight => gracePeriodHeight;

    /// <summary>Returns a 0..1 difficulty value based on world Y position.</summary>
    public float GetDifficulty(float worldY)
    {
        return Mathf.Clamp01(Mathf.Max(0f, worldY) / difficultyRampHeight);
    }

    /// <summary>Is the given height still within the no-hazard grace period?</summary>
    public bool IsInGracePeriod(float worldY)
    {
        return worldY < gracePeriodHeight;
    }

    /// <summary>Returns the moving-platform chance at the given difficulty.</summary>
    public float GetMovingChance(float difficulty, bool inGracePeriod)
    {
        return inGracePeriod ? 0f : Mathf.Lerp(movingChance, movingChance + MovingRampBonus, difficulty);
    }

    /// <summary>Returns the spike-platform chance at the given difficulty.</summary>
    public float GetSpikesChance(float difficulty, bool inGracePeriod)
    {
        return inGracePeriod ? 0f : Mathf.Lerp(spikesChance, spikesChance + SpikesRampBonus, difficulty);
    }

    /// <summary>
    /// Computes the vertical step for the next row.
    /// Scales from ~40% to ~82% of max jump height as difficulty rises.
    /// </summary>
    public float ComputeRowStep(float difficulty, float maxJumpHeight)
    {
        float minFraction = Mathf.Lerp(easyRowFraction,        hardRowFraction * 0.88f, difficulty);
        float maxFraction = Mathf.Lerp(easyRowFraction * 1.35f, hardRowFraction,        difficulty);

        float step = maxJumpHeight * Random.Range(minFraction, maxFraction);
        return Mathf.Clamp(step, 0.7f, maxJumpHeight * MaxSafeRowFraction);
    }

    /// <summary>
    /// Platform count fades from platformsPerRow.y (easy) down to platformsPerRow.x (hard).
    /// </summary>
    public int ComputePlatformCount(float difficulty)
    {
        int maxCount = Mathf.RoundToInt(Mathf.Lerp(platformsPerRow.y, platformsPerRow.x + 0.5f, difficulty));
        int minCount = Mathf.Max(1, maxCount - 1);
        return Random.Range(minCount, maxCount + 1);
    }
}
