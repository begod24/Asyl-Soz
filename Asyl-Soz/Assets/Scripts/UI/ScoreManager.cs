using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Transform player;

    [Header("UI")]
    [UnityEngine.SerializeField] private TMP_Text scoreText;
    [UnityEngine.SerializeField] private TMP_Text bestText;

    [Header("Scoring")]
    [Tooltip("If true: score is measured from starting Y. If false: raw player Y.")]
    [UnityEngine.SerializeField] private bool useStartOffset = true;

    private float startY;
    private float bestScore;
    private float currentScore;

    private const string BestKey = "BEST_SCORE";

    private void Awake()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (player != null) startY = player.position.y;

        bestScore = PlayerPrefs.GetFloat(BestKey, 0f);
        UpdateBestUI(bestScore);
    }

    private void Update()
    {
        if (player == null) return;

        float raw = player.position.y;
        currentScore = useStartOffset ? Mathf.Max(0f, raw - startY) : raw;
        UpdateScoreUI(currentScore);

        if (currentScore > bestScore)
        {
            bestScore = currentScore;
            PlayerPrefs.SetFloat(BestKey, bestScore);
            PlayerPrefs.Save();
            UpdateBestUI(bestScore);
        }
    }

    public float CurrentScore => currentScore;
    public float BestScore => bestScore;

    public void ResetStartY()
    {
        if (player == null) return;
        startY = player.position.y;
        currentScore = 0f;
        UpdateScoreUI(currentScore);
    }

    private void UpdateScoreUI(float score)
    {
        if (scoreText != null)
            scoreText.text = $"{Mathf.FloorToInt(score)} m";
    }

    private void UpdateBestUI(float best)
    {
        if (bestText != null)
            bestText.text = $"Best: {Mathf.FloorToInt(best)} m";
    }
}