using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [UnityEngine.SerializeField] private Transform player;
    [UnityEngine.SerializeField] private Camera cam;
    [UnityEngine.SerializeField] private ScoreManager scoreManager;

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

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

    }

    private void Update()
    {
        if (isGameOver) return;
        if (player == null || cam == null) return;

        float camY = cam.transform.position.y;

        if (player.position.y < camY - fallLimit)
            TriggerGameOver();
    }

    private void TriggerGameOver()
    {
        isGameOver = true;

        // Freeze time (optional)
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}