using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryPanel;
    public GameObject statsPage;
    public GameObject upgradePage;
    public GameObject abilitiesPage;

    [Header("Navigation Buttons")]
    public Button statsButton;
    public Button upgradeButton;
    public Button abilitiesButton;

    [Header("Player References")]
    public HealthSystem playerHealth;
    public PlayerLevelSystem playerLevel;
    public CharacterData playerData;
    public Image playerSprite;
    public PlayerMovement playerMovement;

    [Header("Stats UI Elements")]
    public TMP_Text healthText;
    public TMP_Text expText;
    public TMP_Text levelText;
    public TMP_Text fireLevelText;
    public TMP_Text waterLevelText;
    public TMP_Text earthLevelText;
    public TMP_Text windLevelText;

    [Header("Upgrade UI Elements")]
    public TMP_Text availablePointsText;
    public Button fireUpgradeButton;
    public Button waterUpgradeButton;
    public Button earthUpgradeButton;
    public Button windUpgradeButton;

    [Header("Navigation Settings")]
    public float navigationDelay = 0.2f;
    private float lastNavigationTime = 0f;
    private int currentSelectedIndex = 0;
    private List<Button> navigationButtons = new List<Button>();
    private bool isOnTabNavigation = true;

    public bool isInventoryOpen = false;
    public KeyCode InventoryKey
    {
        get => (KeyCode)PlayerPrefs.GetInt("InventoryKey", (int)KeyCode.I);
        set => PlayerPrefs.SetInt("InventoryKey", (int)value);
    }

    void Start()
    {
        if (playerHealth == null) playerHealth = FindAnyObjectByType<HealthSystem>();
        if (playerLevel == null) playerLevel = FindAnyObjectByType<PlayerLevelSystem>();

        inventoryPanel.SetActive(false);

        if (statsButton != null)
            statsButton.onClick.AddListener(ShowStatsPage);
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(ShowUpgradePage);
        if (abilitiesButton != null)
            abilitiesButton.onClick.AddListener(ShowAbilitiesPage);

        if (fireUpgradeButton != null)
            fireUpgradeButton.onClick.AddListener(() => UpgradeElement(ElementType.Fire));
        if (waterUpgradeButton != null)
            waterUpgradeButton.onClick.AddListener(() => UpgradeElement(ElementType.Water));
        if (earthUpgradeButton != null)
            earthUpgradeButton.onClick.AddListener(() => UpgradeElement(ElementType.Earth));
        if (windUpgradeButton != null)
            windUpgradeButton.onClick.AddListener(() => UpgradeElement(ElementType.Wind));

        InitializeNavigation();
    }

    void InitializeNavigation()
    {
        navigationButtons.Clear();
        if (statsButton != null) navigationButtons.Add(statsButton);
        if (upgradeButton != null) navigationButtons.Add(upgradeButton);
        if (abilitiesButton != null) navigationButtons.Add(abilitiesButton);
        if (navigationButtons.Count > 0)
        {
            navigationButtons[0].Select();
            currentSelectedIndex = 0;
            isOnTabNavigation = true;
        }
    }

    void Update()
    {
        if (GameStateManager.Instance.CurrentState != GameStateManager.GameState.Paused &&
            Input.GetKeyDown(InventoryKey))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            UpdateStatsUI();

            if (isOnTabNavigation)
            {
                HandleNavigationInput();
                HandleTabConfirmation();
            }
            HandleEscapeFromAbilities();
        }
    }

    void HandleNavigationInput()
    {
        if (navigationButtons.Count == 0) return;

        if (Time.unscaledTime - lastNavigationTime < navigationDelay) return;

        float horizontal = Input.GetAxis("Horizontal");
        bool tabPressed = Input.GetKeyDown(KeyCode.Tab);

        if (Mathf.Abs(horizontal) > 0.1f || tabPressed)
        {
            int direction = tabPressed ? 1 : (horizontal > 0 ? 1 : -1);
            NavigateTabs(direction);
            lastNavigationTime = Time.unscaledTime;
        }
    }

    void NavigateTabs(int direction)
    {
        currentSelectedIndex = (currentSelectedIndex + direction + navigationButtons.Count) % navigationButtons.Count;

        if (currentSelectedIndex >= 0 && currentSelectedIndex < navigationButtons.Count)
        {
            navigationButtons[currentSelectedIndex].Select();
        }
    }

    void HandleTabConfirmation()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (currentSelectedIndex >= 0 && currentSelectedIndex < navigationButtons.Count)
            {
                navigationButtons[currentSelectedIndex].onClick.Invoke();
            }
        }
    }

    void HandleEscapeFromAbilities()
    {
        if (abilitiesPage != null && abilitiesPage.activeSelf &&
            (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace)))
        {
            ReturnToTabNavigation();
        }
    }

    void ReturnToTabNavigation()
    {
        isOnTabNavigation = true;
        if (abilitiesButton != null)
        {
            abilitiesButton.Select();
            currentSelectedIndex = 2;
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        if (inventoryPanel != null)
            inventoryPanel.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            ShowStatsPage();
            UpdateStatsUI();
            GameStateManager.Instance.SetState(GameStateManager.GameState.InventoryOpen);

            if (playerMovement != null)
                playerMovement.SetCanMove(false);

            InitializeNavigation();
        }
        else
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);

            if (playerMovement != null)
                playerMovement.SetCanMove(true);

            isOnTabNavigation = true;
        }
    }

    public void UpdateStatsUI()
    {
        if (playerHealth != null && healthText != null)
        {
            healthText.text = $"Здоровье: {playerHealth.CurrentHealth:F0}/{playerHealth.MaxHealth:F0}";
        }

        if (playerLevel != null)
        {
            if (expText != null)
                expText.text = $"Опыт: {playerLevel.CurrentExp}/{playerLevel.ExpToNextLevel}";
            if (levelText != null)
                levelText.text = $"Уровень: {playerLevel.CurrentLevel}";
        }

        if (playerData != null)
        {
            if (fireLevelText != null)
                fireLevelText.text = $"Огонь: {playerData.elementalStats.fireLevel}";
            if (waterLevelText != null)
                waterLevelText.text = $"Вода: {playerData.elementalStats.waterLevel}";
            if (earthLevelText != null)
                earthLevelText.text = $"Земля: {playerData.elementalStats.earthLevel}";
            if (windLevelText != null)
                windLevelText.text = $"Ветер: {playerData.elementalStats.windLevel}";
        }

        UpdateUpgradeUI();
    }

    void UpdateUpgradeUI()
    {
        if (playerLevel != null && playerData != null)
        {
            if (availablePointsText != null)
                availablePointsText.text = $"Очков прокачки: {playerLevel.AvailableSkillPoints}";

            if (fireUpgradeButton != null)
                fireUpgradeButton.interactable = playerLevel.AvailableSkillPoints > 0 &&
                                               playerData.elementalStats.fireLevel < 5;
            if (waterUpgradeButton != null)
                waterUpgradeButton.interactable = playerLevel.AvailableSkillPoints > 0 &&
                                                playerData.elementalStats.waterLevel < 5;
            if (earthUpgradeButton != null)
                earthUpgradeButton.interactable = playerLevel.AvailableSkillPoints > 0 &&
                                                playerData.elementalStats.earthLevel < 5;
            if (windUpgradeButton != null)
                windUpgradeButton.interactable = playerLevel.AvailableSkillPoints > 0 &&
                                               playerData.elementalStats.windLevel < 5;
        }
    }

    public void ShowStatsPage()
    {
        if (statsPage != null) statsPage.SetActive(true);
        if (upgradePage != null) upgradePage.SetActive(false);
        if (abilitiesPage != null) abilitiesPage.SetActive(false);
        currentSelectedIndex = 0;
        if (statsButton != null) statsButton.Select();
        isOnTabNavigation = true;
    }

    public void ShowUpgradePage()
    {
        if (statsPage != null) statsPage.SetActive(false);
        if (upgradePage != null) upgradePage.SetActive(true);
        if (abilitiesPage != null) abilitiesPage.SetActive(false);
        currentSelectedIndex = 1;
        if (upgradeButton != null) upgradeButton.Select();
        UpdateUpgradeUI();
        isOnTabNavigation = true;
    }

    public void ShowAbilitiesPage()
    {
        if (statsPage != null) statsPage.SetActive(false);
        if (upgradePage != null) upgradePage.SetActive(false);
        if (abilitiesPage != null) abilitiesPage.SetActive(true);
        currentSelectedIndex = 2;
        if (abilitiesButton != null) abilitiesButton.Select();
        isOnTabNavigation = false;
    }

    void UpgradeElement(ElementType element)
    {
        if (playerLevel != null && playerData != null)
        {
            if (playerLevel.UseSkillPoint(1))
            {
                playerData.elementalStats.UpgradeElement(element);
                UpdateStatsUI();
                Debug.Log($"Улучшена стихия {element} до уровня {playerData.elementalStats.GetElementLevel(element)}");
            }
        }
    }

    public void OnStatsButtonClick()
    {
        ShowStatsPage();
    }

    public void OnUpgradeButtonClick()
    {
        ShowUpgradePage();
    }

    public void OnAbilitiesButtonClick()
    {
        ShowAbilitiesPage();
    }

    public void OnCloseButtonClick()
    {
        ToggleInventory();
    }

    public void SetInventoryKey(KeyCode newKey)
    {
        InventoryKey = newKey;
        PlayerPrefs.Save();
    }

    public KeyCode GetInventoryKey()
    {
        return InventoryKey;
    }

    [ContextMenu("Check Missing References")]
    void CheckMissingReferences()
    {
        Debug.Log("Проверка ссылок в InventorySystem:");
        Debug.Log($"playerHealth: {playerHealth != null}");
        Debug.Log($"playerLevel: {playerLevel != null}");
        Debug.Log($"playerData: {playerData != null}");
        Debug.Log($"playerSprite: {playerSprite != null}");
        Debug.Log($"healthText: {healthText != null}");
        Debug.Log($"expText: {expText != null}");
        Debug.Log($"levelText: {levelText != null}");
    }
}