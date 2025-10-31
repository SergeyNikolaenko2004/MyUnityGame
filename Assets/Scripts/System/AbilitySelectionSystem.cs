using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitySelectionSystem : MonoBehaviour
{
    [Header("References")]
    public GameObject abilitiesPage;
    public Transform abilitiesContainer;
    public Transform selectedAbilitiesContainer;
    public GameObject abilityButtonPrefab;
    public CharacterData playerData;
    public List<AbilityData> allAbilities;

    [Header("Gameplay Ability Panel")]
    public Image[] gameplayAbilityImages;
    public Image[] cooldownOverlays;
    public Sprite emptySlotSprite; 

    private List<AbilityData> selectedAbilities = new List<AbilityData>();
    private List<AbilityButton> abilityButtons = new List<AbilityButton>();
    private List<AbilityButton> slotButtons = new List<AbilityButton>();
    private List<IAbility> abilityInstances = new List<IAbility>();

    void Start()
    {
        LoadSelectedAbilities();
        InitializeAbilityButtons();
        FindAbilityInstances();
        UpdateUI();
        UpdateGameplayAbilityPanel();
    }

    void Update()
    {
        UpdateCooldownDisplay();
    }

    public List<AbilityData> GetSelectedAbilities()
    {
        return selectedAbilities;
    }

    void FindAbilityInstances()
    {
        abilityInstances.Clear();

        var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var obj in allObjects)
        {
            if (obj is IAbility ability)
            {
                abilityInstances.Add(ability);
            }
        }
    }

    void UpdateCooldownDisplay()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < selectedAbilities.Count && selectedAbilities[i] != null)
            {
                var abilityInstance = FindAbilityInstance(selectedAbilities[i]);
                if (abilityInstance != null)
                {
                    float cooldownRemaining = abilityInstance.GetCooldownRemaining();
                    float cooldownTotal = abilityInstance.GetCooldownTotal();

                    if (cooldownRemaining > 0 && cooldownTotal > 0)
                    {
                        if (cooldownOverlays[i] != null)
                        {
                            cooldownOverlays[i].fillAmount = cooldownRemaining / cooldownTotal;
                            cooldownOverlays[i].gameObject.SetActive(true);
                        }
                    }
                    else
                    {
                        if (cooldownOverlays[i] != null)
                            cooldownOverlays[i].gameObject.SetActive(false);
                    }
                }
                else
                {
                    if (cooldownOverlays[i] != null)
                        cooldownOverlays[i].gameObject.SetActive(false);
                }
            }
            else
            {
                if (cooldownOverlays[i] != null)
                    cooldownOverlays[i].gameObject.SetActive(false);
            }
        }
    }

    IAbility FindAbilityInstance(AbilityData abilityData)
    {
        var currentAbilityInstances = new List<IAbility>();
        var allObjects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj is IAbility ability)
            {
                currentAbilityInstances.Add(ability);

                if (ability.GetAbilityData() == abilityData)
                {
                    return ability;
                }
            }
        }

        Debug.LogWarning($"Не найдена способность: {abilityData.abilityName}. Всего способностей в сцене: {currentAbilityInstances.Count}");
        return null;
    }

    void LoadSelectedAbilities()
    {
        selectedAbilities.Clear();

        for (int i = 0; i < 4; i++)
        {
            string abilityName = PlayerPrefs.GetString($"SelectedAbility_{i}", "");
            if (!string.IsNullOrEmpty(abilityName))
            {
                var ability = allAbilities.Find(a => a != null && a.abilityName == abilityName);
                if (ability != null)
                {
                    selectedAbilities.Add(ability);
                    Debug.Log($"Загружена способность: {ability.abilityName} в слот {i}");
                }
            }
        }

        if (selectedAbilities.Count > 0)
        {
            playerData.abilities = selectedAbilities.ToArray();
        }
    }

    void InitializeAbilityButtons()
    {
        foreach (Transform child in abilitiesContainer)
            Destroy(child.gameObject);
        foreach (Transform child in selectedAbilitiesContainer)
            Destroy(child.gameObject);

        abilityButtons.Clear();
        slotButtons.Clear();

        Debug.Log($"Создаем кнопки для {allAbilities.Count} способностей");

        foreach (var ability in allAbilities)
        {
            if (ability == null)
            {
                Debug.LogWarning("Пропускаем null способность");
                continue;
            }

            GameObject buttonObj = Instantiate(abilityButtonPrefab, abilitiesContainer);
            AbilityButton abilityButton = buttonObj.GetComponent<AbilityButton>();
            if (abilityButton != null)
            {
                abilityButton.Initialize(ability, this, false);
                abilityButtons.Add(abilityButton);
                Debug.Log($"Создана кнопка для: {ability.abilityName}");
            }
            else
            {
                Debug.LogError("AbilityButton компонент не найден на префабе!");
            }
        }

        for (int i = 0; i < 4; i++)
        {
            GameObject slotObj = Instantiate(abilityButtonPrefab, selectedAbilitiesContainer);
            AbilityButton slotButton = slotObj.GetComponent<AbilityButton>();
            if (slotButton != null)
            {
                slotButton.InitializeAsSlot(i, this);
                slotButtons.Add(slotButton);

                if (i < selectedAbilities.Count && selectedAbilities[i] != null)
                {
                    slotButton.SetAbility(selectedAbilities[i]);
                    Debug.Log($"Установлена способность в слот {i}: {selectedAbilities[i].abilityName}");
                }
                else
                {
                    slotButton.ClearAbility();
                }
            }
        }
    }

    public void OnAbilityClicked(AbilityData ability, bool isSlot)
    {
        Debug.Log($"Клик по способности: {ability?.abilityName}, isSlot: {isSlot}");

        if (ability == null)
        {
            Debug.LogWarning("Клик по null способности!");
            return;
        }

        if (isSlot)
        {
            RemoveAbilityFromSlot(ability);
        }
        else
        {
            AddAbilityToSlot(ability);
        }
        SaveSelectedAbilities();
    }

    void AddAbilityToSlot(AbilityData ability)
    {
        if (selectedAbilities.Count >= 4)
        {
            Debug.Log("Максимум 4 способности!");
            return;
        }

        if (selectedAbilities.Contains(ability))
        {
            Debug.Log("Способность уже выбрана!");
            return;
        }

        selectedAbilities.Add(ability);
        Debug.Log($"Добавлена способность: {ability.abilityName}, всего выбрано: {selectedAbilities.Count}");
        UpdateUI();
        UpdateGameplayAbilityPanel();
    }

    void RemoveAbilityFromSlot(AbilityData ability)
    {
        if (selectedAbilities.Contains(ability))
        {
            selectedAbilities.Remove(ability);
            Debug.Log($"Удалена способность: {ability.abilityName}, всего выбрано: {selectedAbilities.Count}");
            UpdateUI();
            UpdateGameplayAbilityPanel();
        }
    }

    void SaveSelectedAbilities()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i < selectedAbilities.Count && selectedAbilities[i] != null)
            {
                PlayerPrefs.SetString($"SelectedAbility_{i}", selectedAbilities[i].abilityName);
                Debug.Log($"Сохранен слот {i}: {selectedAbilities[i].abilityName}");
            }
            else
            {
                PlayerPrefs.DeleteKey($"SelectedAbility_{i}");
                Debug.Log($"Очищен слот {i}");
            }
        }
        PlayerPrefs.Save();
        playerData.abilities = selectedAbilities.ToArray();

        Debug.Log("Способности сохранены!");
        UpdateAllAbilitiesInScene();
        FindAbilityInstances();
    }

    void UpdateAllAbilitiesInScene()
    {
        var allAbilitiesScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var script in allAbilitiesScripts)
        {
            if (script is IAbility abilityScript)
            {
                abilityScript.UpdateAbilityState();
            }
        }
    }

    public void UpdateAllAbilityDamageDisplays()
    {
        foreach (var abilityButton in abilityButtons)
        {
            if (abilityButton != null)
            {
                abilityButton.UpdateDamageDisplay();
            }
        }

        foreach (var slotButton in slotButtons)
        {
            if (slotButton != null && slotButton.GetAbilityData() != null)
            {
                slotButton.UpdateDamageDisplay();
            }
        }
    }

    // Обновите метод UpdateUI
    void UpdateUI()
    {
        Debug.Log($"Обновление UI: выбрано {selectedAbilities.Count} способностей");

        for (int i = 0; i < slotButtons.Count; i++)
        {
            if (i < selectedAbilities.Count && selectedAbilities[i] != null)
            {
                slotButtons[i].SetAbility(selectedAbilities[i]);
                Debug.Log($"Слот {i}: {selectedAbilities[i].abilityName}");
            }
            else
            {
                slotButtons[i].ClearAbility();
                Debug.Log($"Слот {i}: пустой");
            }
        }

        foreach (var abilityButton in abilityButtons)
        {
            var abilityData = abilityButton.GetAbilityData();
            if (abilityData != null)
            {
                bool isSelected = selectedAbilities.Contains(abilityData);
                abilityButton.SetSelected(isSelected);
                abilityButton.UpdateDamageDisplay(); // Обновляем отображение урона
                Debug.Log($"Кнопка {abilityData.abilityName}: выбрана = {isSelected}");
            }
        }
    }

    void UpdateGameplayAbilityPanel()
    {
        if (gameplayAbilityImages == null || gameplayAbilityImages.Length != 4)
        {
            Debug.LogWarning("Gameplay ability images not properly configured!");
            return;
        }

        for (int i = 0; i < 4; i++)
        {
            if (i < selectedAbilities.Count && selectedAbilities[i] != null)
            {
      
                gameplayAbilityImages[i].sprite = selectedAbilities[i].icon;
                gameplayAbilityImages[i].color = Color.white; 
                Debug.Log($"Установлена иконка в слот {i}: {selectedAbilities[i].abilityName}");
            }
            else
            {

                if (emptySlotSprite != null)
                {
                    gameplayAbilityImages[i].sprite = emptySlotSprite;
                }
                else
                {
                    gameplayAbilityImages[i].sprite = null;
                    gameplayAbilityImages[i].color = new Color(1, 1, 1, 0.2f); 
                }
                Debug.Log($"Очищен слот {i}");
            }
        }
    }
    public void RefreshUI()
    {
        LoadSelectedAbilities();
        UpdateUI();
        UpdateGameplayAbilityPanel();
    }


    [ContextMenu("Проверить настройки")]
    void CheckSettings()
    {
        Debug.Log("=== ПРОВЕРКА НАСТРОЕК ===");
        Debug.Log($"All Abilities count: {allAbilities.Count}");
        Debug.Log($"Abilities Container: {abilitiesContainer}");
        Debug.Log($"Selected Container: {selectedAbilitiesContainer}");
        Debug.Log($"Button Prefab: {abilityButtonPrefab}");
        Debug.Log($"Player Data: {playerData}");
        Debug.Log($"Gameplay Ability Images: {gameplayAbilityImages?.Length}");

        foreach (var ability in allAbilities)
        {
            if (ability != null)
                Debug.Log($"Ability: {ability.abilityName}");
            else
                Debug.LogWarning("NULL ability in list!");
        }
    }
}

public interface IAbility
{
    void UpdateAbilityState();
    float GetCooldownRemaining();
    float GetCooldownTotal();
    AbilityData GetAbilityData();
    int GetAbilityIndex();
    void ReduceCooldown(float reductionAmount);
    float GetCurrentCooldown();
    bool IsAbilityReady();
}