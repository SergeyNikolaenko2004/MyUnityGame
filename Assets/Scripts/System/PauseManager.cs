using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenu;
    public Button settingsButton;
    public Button mainMenuButton;
    public Button resumeButton;
    public GameObject settingsPanel;
    public GameObject videoSettingsPanel;
    public GameObject audioSettingsPanel;
    public GameObject controlsSettingsPanel;

    [Header("Settings Tab Buttons")]
    public Button videoTabButton;
    public Button audioTabButton;
    public Button controlsTabButton;

    [Header("Video Settings Reference")]
    public VideoSettingsManager videoSettingsManager;

    [Header("Input Settings Reference")]
    public InputSettingsSystem inputSettingsSystem;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;
    private static PauseManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        resumeButton.onClick.AddListener(ResumeGame);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
        settingsButton.onClick.AddListener(OpenSettings);

        if (videoTabButton != null)
            videoTabButton.onClick.AddListener(OpenVideoSettings);
        if (audioTabButton != null)
            audioTabButton.onClick.AddListener(OpenAudioSettings);
        if (controlsTabButton != null)
            controlsTabButton.onClick.AddListener(OpenControlsSettings);

        pauseMenu.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isPaused)
        {
            ResumeGame();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsMainMenuScene())
        {
            if (inputSettingsSystem != null && inputSettingsSystem.IsRebindingActive())
            {
                inputSettingsSystem.CancelRebinding();
                return;
            }

            if (videoSettingsManager != null && videoSettingsManager.IsDropdownOpen())
            {
                videoSettingsManager.CloseDropdown();
                return;
            }

            if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.InventoryOpen)
            {
                HandleEscapeKey();
            }
        }
    }

    void HandleEscapeKey()
    {
        if (inputSettingsSystem != null && inputSettingsSystem.IsRebindingActive())
        {
            inputSettingsSystem.CancelRebinding();
            return;
        }

        if (videoSettingsManager != null && videoSettingsManager.IsDropdownOpen())
        {
            videoSettingsManager.CloseDropdown();
            return;
        }

        if (videoSettingsPanel != null && videoSettingsPanel.activeSelf)
        {
            CloseVideoSettings();
        }
        else if (audioSettingsPanel != null && audioSettingsPanel.activeSelf)
        {
            CloseAudioSettings();
        }
        else if (controlsSettingsPanel != null && controlsSettingsPanel.activeSelf)
        {
            CloseControlsSettings();
        }
        else if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
        }
        else if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    bool IsMainMenuScene()
    {
        return SceneManager.GetActiveScene().name == mainMenuSceneName;
    }

    public void PauseGame()
    {
        if (IsMainMenuScene()) return;

        isPaused = true;
        GameStateManager.Instance.SetState(GameStateManager.GameState.Paused);
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        pauseMenu.SetActive(false);
        CloseAllSettingsPanels();
        Time.timeScale = 1f;

        if (inputSettingsSystem != null)
        {
            inputSettingsSystem.CloseSettings();
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            CloseAllSubSettingsPanels();
            if (videoSettingsPanel != null) videoSettingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenVideoSettings()
    {
        if (videoSettingsPanel != null)
        {
            videoSettingsPanel.SetActive(true);
            if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
            if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);

        }
    }

    public void CloseVideoSettings()
    {
        if (videoSettingsPanel != null)
        {
            videoSettingsPanel.SetActive(false);

        }
    }

    public void OpenAudioSettings()
    {
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.SetActive(true);
            if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);
            if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
        }
    }

    public void CloseAudioSettings()
    {
        if (audioSettingsPanel != null)
        {
            audioSettingsPanel.SetActive(false);
        }
    }

    public void OpenControlsSettings()
    {
        if (controlsSettingsPanel != null)
        {
            controlsSettingsPanel.SetActive(true);
            if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
            if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);

        }
    }

    public void CloseControlsSettings()
    {
        if (controlsSettingsPanel != null)
        {
            controlsSettingsPanel.SetActive(false);

            if (inputSettingsSystem != null)
            {
                inputSettingsSystem.CloseSettings(false);
            }
        }
    }

    void CloseAllSubSettingsPanels()
    {
        if (videoSettingsPanel != null)
        {
            videoSettingsPanel.SetActive(false);
        }
        if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
        if (controlsSettingsPanel != null) controlsSettingsPanel.SetActive(false);
    }

    void CloseAllSettingsPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        CloseAllSubSettingsPanels();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Destroy(player);
        }

        SceneManager.LoadScene("MainMenu");
        pauseMenu.SetActive(false);
        CloseAllSettingsPanels();
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

}