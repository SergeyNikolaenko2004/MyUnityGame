using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class InputSettingsSystem : MonoBehaviour
{
    [Header("UI References")]
    public GameObject settingsPanel;
    public Button jumpKeyButton;
    public Button rightKeyButton;
    public Button leftKeyButton;
    public TMP_Text jumpKeyText;
    public TMP_Text rightKeyText;
    public TMP_Text leftKeyText;
    public GameObject keyRebindPanel;
    public TMP_Text rebindPromptText;

    [Header("Inventory Settings")]
    public Button inventoryKeyButton;
    public TMP_Text inventoryKeyText;

    [Header("Ability Settings")]
    public Button[] abilityKeyButtons = new Button[4];
    public TMP_Text[] abilityKeyTexts = new TMP_Text[4];

    [Header("Player Reference")]
    public PlayerMovement playerMovement;
    public AbilitySelectionSystem abilitySelectionSystem;

    private Dictionary<Button, System.Action<KeyCode>> buttonActions;
    private Button currentRebindButton;
    private bool isRebinding = false;
    private int currentAbilitySlot = -1;
    private bool shouldChangeGameState = true;

    private KeyCode[] defaultAbilityKeys = new KeyCode[] { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L };

    private void Start()
    {
        settingsPanel.SetActive(false);
        keyRebindPanel.SetActive(false);

        buttonActions = new Dictionary<Button, System.Action<KeyCode>>
        {
            { jumpKeyButton, (key) => SetJumpKey(key) },
            { rightKeyButton, (key) => SetRightKey(key) },
            { leftKeyButton, (key) => SetLeftKey(key) }
        };

        if (inventoryKeyButton != null)
        {
            inventoryKeyButton.onClick.AddListener(() => StartRebinding(inventoryKeyButton, "Инвентарь"));
            buttonActions.Add(inventoryKeyButton, (key) => SetInventoryKey(key));
        }

        for (int i = 0; i < 4; i++)
        {
            int slotIndex = i;
            if (abilityKeyButtons[i] != null && abilityKeyTexts[i] != null)
            {
                abilityKeyButtons[i].onClick.AddListener(() => StartAbilityRebinding(slotIndex));
            }
        }

        jumpKeyButton.onClick.AddListener(() => StartRebinding(jumpKeyButton, "Прыжок"));
        rightKeyButton.onClick.AddListener(() => StartRebinding(rightKeyButton, "Вправо"));
        leftKeyButton.onClick.AddListener(() => StartRebinding(leftKeyButton, "Влево"));

        LoadCurrentKeys();
    }

    private void Update()
    {
        if (isRebinding)
        {
            CheckForKeyPress();
        }
    }

    private void LoadCurrentKeys()
    {
        jumpKeyText.text = playerMovement.JumpKey.ToString();
        rightKeyText.text = GetKeyCodeFromAxis("Right").ToString();
        leftKeyText.text = GetKeyCodeFromAxis("Left").ToString();

        if (inventoryKeyText != null)
        {
            InventorySystem inventorySystem = FindAnyObjectByType<InventorySystem>();
            if (inventorySystem != null)
            {
                inventoryKeyText.text = inventorySystem.GetInventoryKey().ToString();
            }
        }

        for (int i = 0; i < 4; i++)
        {
            if (abilityKeyTexts[i] != null)
            {
                KeyCode abilityKey = (KeyCode)PlayerPrefs.GetInt($"AbilityKey_{i}", (int)defaultAbilityKeys[i]);
                abilityKeyTexts[i].text = abilityKey.ToString();
            }
        }
    }

    private void StartRebinding(Button button, string actionName)
    {
        currentRebindButton = button;
        currentAbilitySlot = -1;
        isRebinding = true;
        keyRebindPanel.SetActive(true);
        rebindPromptText.text = $"Нажмите клавишу для {actionName}...";
    }

    private void StartAbilityRebinding(int slotIndex)
    {
        currentRebindButton = null;
        currentAbilitySlot = slotIndex;
        isRebinding = true;
        keyRebindPanel.SetActive(true);

        string abilityName = GetAbilityNameInSlot(slotIndex);
        rebindPromptText.text = $"Нажмите клавишу для способности {slotIndex + 1} ({abilityName})...";
    }

    private string GetAbilityNameInSlot(int slotIndex)
    {
        if (abilitySelectionSystem != null)
        {
            var selectedAbilities = abilitySelectionSystem.GetSelectedAbilities();
            if (slotIndex < selectedAbilities.Count && selectedAbilities[slotIndex] != null)
            {
                return selectedAbilities[slotIndex].abilityName;
            }
        }
        return "Пусто";
    }

    private void CheckForKeyPress()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    if (keyCode == KeyCode.Escape)
                    {
                        CancelRebinding();
                        return;
                    }

                    if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter ||
                        keyCode == KeyCode.Tab || keyCode == KeyCode.UpArrow ||
                        keyCode == KeyCode.DownArrow || keyCode == KeyCode.LeftArrow ||
                        keyCode == KeyCode.RightArrow)
                    {
                        return;
                    }

                    CompleteRebinding(keyCode);
                    return;
                }
            }
        }
    }

    private void CompleteRebinding(KeyCode newKey)
    {
        if (currentAbilitySlot >= 0)
        {
            SetAbilityKey(currentAbilitySlot, newKey);
        }
        else if (currentRebindButton != null && buttonActions.ContainsKey(currentRebindButton))
        {
            buttonActions[currentRebindButton](newKey);
        }

        isRebinding = false;
        keyRebindPanel.SetActive(false);
        LoadCurrentKeys();
    }

    public void CancelRebinding()
    {
        isRebinding = false;
        keyRebindPanel.SetActive(false);
    }

    private void SetJumpKey(KeyCode newKey)
    {
        playerMovement.JumpKey = newKey;
        PlayerPrefs.SetInt("JumpKey", (int)newKey);
        PlayerPrefs.Save();
    }

    private void SetRightKey(KeyCode newKey)
    {
        PlayerPrefs.SetInt("RightKey", (int)newKey);
        UpdateAxisKeys();
    }

    private void SetLeftKey(KeyCode newKey)
    {
        PlayerPrefs.SetInt("LeftKey", (int)newKey);
        UpdateAxisKeys();
    }

    private void SetInventoryKey(KeyCode newKey)
    {
        InventorySystem inventorySystem = FindAnyObjectByType<InventorySystem>();
        if (inventorySystem != null)
        {
            inventorySystem.SetInventoryKey(newKey);
        }
    }

    private void SetAbilityKey(int slotIndex, KeyCode newKey)
    {
        PlayerPrefs.SetInt($"AbilityKey_{slotIndex}", (int)newKey);
        PlayerPrefs.Save();
        UpdateAllAbilitiesInScene();
    }

    private void UpdateAllAbilitiesInScene()
    {
        var allAbilities = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allAbilities)
        {
            if (obj is IAbility ability)
            {
                ability.UpdateAbilityState();
            }
        }
    }

    private void UpdateAxisKeys()
    {
        PlayerPrefs.Save();
    }

    private KeyCode GetKeyCodeFromAxis(string direction)
    {
        return (KeyCode)PlayerPrefs.GetInt(direction + "Key", direction == "Right" ? (int)KeyCode.D : (int)KeyCode.A);
    }

    public static KeyCode GetAbilityKey(int slotIndex)
    {
        KeyCode[] defaultKeys = { KeyCode.H, KeyCode.J, KeyCode.K, KeyCode.L };
        if (slotIndex >= 0 && slotIndex < 4)
        {
            return (KeyCode)PlayerPrefs.GetInt($"AbilityKey_{slotIndex}", (int)defaultKeys[slotIndex]);
        }
        return KeyCode.None;
    }

    public void OpenSettings(bool changeGameState = true)
    {
        settingsPanel.SetActive(true);
        LoadCurrentKeys();
        shouldChangeGameState = changeGameState;

        if (shouldChangeGameState)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.InventoryOpen);
        }

        if (playerMovement != null && shouldChangeGameState)
        {
            playerMovement.SetCanMove(false);
        }
    }

    public void CloseSettings(bool changeGameState = true)
    {
        settingsPanel.SetActive(false);
        if (isRebinding)
        {
            CancelRebinding();
        }

        if (shouldChangeGameState && changeGameState)
        {
            GameStateManager.Instance.SetState(GameStateManager.GameState.Playing);
        }

        if (playerMovement != null && shouldChangeGameState && changeGameState)
        {
            playerMovement.SetCanMove(true);
        }
    }

    public void ResetToDefaults()
    {
        SetJumpKey(KeyCode.W);
        SetRightKey(KeyCode.D);
        SetLeftKey(KeyCode.A);

        InventorySystem inventorySystem = FindAnyObjectByType<InventorySystem>();
        if (inventorySystem != null)
        {
            inventorySystem.SetInventoryKey(KeyCode.I);
        }

        for (int i = 0; i < 4; i++)
        {
            SetAbilityKey(i, defaultAbilityKeys[i]);
        }

        LoadCurrentKeys();
        UpdateAllAbilitiesInScene();
    }

    public bool IsRebindingActive()
    {
        return isRebinding;
    }
}