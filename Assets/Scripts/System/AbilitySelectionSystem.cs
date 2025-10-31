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

        Debug.LogWarning($"�� ������� �����������: {abilityData.abilityName}. ����� ������������ � �����: {currentAbilityInstances.Count}");
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
                    Debug.Log($"��������� �����������: {ability.abilityName} � ���� {i}");
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

        Debug.Log($"������� ������ ��� {allAbilities.Count} ������������");

        foreach (var ability in allAbilities)
        {
            if (ability == null)
            {
                Debug.LogWarning("���������� null �����������");
                continue;
            }

            GameObject buttonObj = Instantiate(abilityButtonPrefab, abilitiesContainer);
            AbilityButton abilityButton = buttonObj.GetComponent<AbilityButton>();
            if (abilityButton != null)
            {
                abilityButton.Initialize(ability, this, false);
                abilityButtons.Add(abilityButton);
                Debug.Log($"������� ������ ���: {ability.abilityName}");
            }
            else
            {
                Debug.LogError("AbilityButton ��������� �� ������ �� �������!");
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
                    Debug.Log($"����������� ����������� � ���� {i}: {selectedAbilities[i].abilityName}");
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
        Debug.Log($"���� �� �����������: {ability?.abilityName}, isSlot: {isSlot}");

        if (ability == null)
        {
            Debug.LogWarning("���� �� null �����������!");
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
            Debug.Log("�������� 4 �����������!");
            return;
        }

        if (selectedAbilities.Contains(ability))
        {
            Debug.Log("����������� ��� �������!");
            return;
        }

        selectedAbilities.Add(ability);
        Debug.Log($"��������� �����������: {ability.abilityName}, ����� �������: {selectedAbilities.Count}");
        UpdateUI();
        UpdateGameplayAbilityPanel();
    }

    void RemoveAbilityFromSlot(AbilityData ability)
    {
        if (selectedAbilities.Contains(ability))
        {
            selectedAbilities.Remove(ability);
            Debug.Log($"������� �����������: {ability.abilityName}, ����� �������: {selectedAbilities.Count}");
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
                Debug.Log($"�������� ���� {i}: {selectedAbilities[i].abilityName}");
            }
            else
            {
                PlayerPrefs.DeleteKey($"SelectedAbility_{i}");
                Debug.Log($"������ ���� {i}");
            }
        }
        PlayerPrefs.Save();
        playerData.abilities = selectedAbilities.ToArray();

        Debug.Log("����������� ���������!");
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

    // �������� ����� UpdateUI
    void UpdateUI()
    {
        Debug.Log($"���������� UI: ������� {selectedAbilities.Count} ������������");

        for (int i = 0; i < slotButtons.Count; i++)
        {
            if (i < selectedAbilities.Count && selectedAbilities[i] != null)
            {
                slotButtons[i].SetAbility(selectedAbilities[i]);
                Debug.Log($"���� {i}: {selectedAbilities[i].abilityName}");
            }
            else
            {
                slotButtons[i].ClearAbility();
                Debug.Log($"���� {i}: ������");
            }
        }

        foreach (var abilityButton in abilityButtons)
        {
            var abilityData = abilityButton.GetAbilityData();
            if (abilityData != null)
            {
                bool isSelected = selectedAbilities.Contains(abilityData);
                abilityButton.SetSelected(isSelected);
                abilityButton.UpdateDamageDisplay(); // ��������� ����������� �����
                Debug.Log($"������ {abilityData.abilityName}: ������� = {isSelected}");
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
                Debug.Log($"����������� ������ � ���� {i}: {selectedAbilities[i].abilityName}");
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
                Debug.Log($"������ ���� {i}");
            }
        }
    }
    public void RefreshUI()
    {
        LoadSelectedAbilities();
        UpdateUI();
        UpdateGameplayAbilityPanel();
    }


    [ContextMenu("��������� ���������")]
    void CheckSettings()
    {
        Debug.Log("=== �������� �������� ===");
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