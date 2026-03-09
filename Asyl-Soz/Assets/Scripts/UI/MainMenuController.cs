using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels (assign in Inspector)")]
    [UnityEngine.SerializeField] private GameObject settingsPanel;
    [UnityEngine.SerializeField] private GameObject creditsPanel;

    private void Awake()
    {
        // Optional auto-find by name if not assigned
        if (settingsPanel == null)
        {
            var s = GameObject.Find("SettingsPanel");
            if (s != null) settingsPanel = s;
        }

        if (creditsPanel == null)
        {
            var c = GameObject.Find("CreditsPanel");
            if (c != null) creditsPanel = c;
        }
    }

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        else Debug.LogWarning("MainMenuController: settingsPanel is NOT assigned.");

        if (creditsPanel != null) creditsPanel.SetActive(false);
        else Debug.LogWarning("MainMenuController: creditsPanel is NOT assigned.");

        Time.timeScale = 1f;
    }

    public void Play()
    {
        Debug.Log("MainMenuController: Play()");
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void OpenSettings()
    {
        Debug.Log("MainMenuController: OpenSettings()");
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        Debug.Log("MainMenuController: CloseSettings()");
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    public void OpenCredits()
    {
        Debug.Log("MainMenuController: OpenCredits()");
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        Debug.Log("MainMenuController: CloseCredits()");
        if (creditsPanel != null) creditsPanel.SetActive(false);
        else Debug.LogWarning("MainMenuController: creditsPanel is NULL (not assigned).");
    }

    public void QuitGame()
    {
        Debug.Log("MainMenuController: QuitGame()");
        Application.Quit();
    }
}