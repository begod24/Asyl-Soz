using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Transform player;
    [UnityEngine.SerializeField] private Camera cam;
    [UnityEngine.SerializeField] private ScoreManager scoreManager;

    [Header("Health (optional)")]
    [UnityEngine.SerializeField] private PlayerHealth playerHealth;

    [Header("Game Over Settings")]
    [Tooltip("Game over if player goes this far below camera.")]
    [UnityEngine.SerializeField] private float fallLimit = 8f;

    [Header("UI")]
    [UnityEngine.SerializeField] private GameObject gameOverPanel;
    [UnityEngine.SerializeField] private TMP_Text finalScoreText;
    [UnityEngine.SerializeField] private TMP_Text finalBestText;

    private bool isGameOver;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        // Find PlayerHealth if not assigned
        if (playerHealth == null && player != null)
            playerHealth = player.GetComponent<PlayerHealth>();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        // Ensure unpaused at scene start
        Time.timeScale = 1f;
    }

    private void OnEnable()
    {
        // Subscribe to death
        if (playerHealth != null)
            playerHealth.OnDied += OnPlayerDied;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= OnPlayerDied;
    }

    private void Update()
    {
        if (isGameOver) return;
        if (player == null || cam == null) return;

        float camY = cam.transform.position.y;

        if (player.position.y < camY - fallLimit)
            TriggerGameOver();
    }

    private void OnPlayerDied()
    {
        if (isGameOver) return;
        TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        // Freeze time
        Time.timeScale = 0f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        float score = scoreManager != null ? scoreManager.CurrentScore : 0f;
        float best = scoreManager != null ? scoreManager.BestScore : 0f;

        if (finalScoreText != null)
            finalScoreText.text = $"Score: {Mathf.FloorToInt(score)} m";

        if (finalBestText != null)
            finalBestText.text = $"Best: {Mathf.FloorToInt(best)} m";
    }

    public void Restart()
    {
        // IMPORTANT: clear saved HP so you don't restart with 0 or old value
        PlayerHealth.ClearSavedHealth();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}