using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    [Header("UI Elements")]
    public Image abilityIcon;
    public TMP_Text abilityNameText;
    public TMP_Text elementText;
    public TMP_Text damageText;
    public Button button;
    public GameObject selectedIndicator;

    private AbilityData abilityData;
    private AbilitySelectionSystem selectionSystem;
    private bool isSlot = false;
    private int slotIndex = -1;
    private CharacterData playerData;

    public void Initialize(AbilityData data, AbilitySelectionSystem system, bool isSlotButton)
    {
        abilityData = data;
        selectionSystem = system;
        isSlot = isSlotButton;
        playerData = system.playerData; // �������� ������ �� playerData

        UpdateUI();

        if (button != null) button.onClick.AddListener(OnClick);
        if (selectedIndicator != null) selectedIndicator.SetActive(false);
    }

    public void InitializeAsSlot(int index, AbilitySelectionSystem system)
    {
        isSlot = true;
        slotIndex = index;
        selectionSystem = system;
        playerData = system.playerData; // �������� ������ �� playerData

        if (abilityIcon != null) abilityIcon.sprite = null;
        if (abilityNameText != null) abilityNameText.text = $"";
        if (elementText != null) elementText.text = "";
        if (damageText != null) damageText.text = "";
        if (selectedIndicator != null) selectedIndicator.SetActive(false);

        if (button != null) button.onClick.AddListener(OnClick);
    }

    public void SetAbility(AbilityData data)
    {
        abilityData = data;
        UpdateUI(); // ��������� UI ��� ��������� �����������
        if (selectedIndicator != null) selectedIndicator.SetActive(true);
    }

    public void ClearAbility()
    {
        abilityData = null;
        if (abilityIcon != null) abilityIcon.sprite = null;
        if (abilityNameText != null) abilityNameText.text = $"";
        if (elementText != null) elementText.text = "";
        if (damageText != null) damageText.text = "";
        if (selectedIndicator != null) selectedIndicator.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (!isSlot && selectedIndicator != null)
        {
            selectedIndicator.SetActive(selected);
        }
    }

    public AbilityData GetAbilityData()
    {
        return abilityData;
    }

    // ����� ����� ��� ���������� ����� � ����������� �� ������ ������
    public void UpdateDamageDisplay()
    {
        if (abilityData != null && damageText != null && playerData != null)
        {
            int currentDamage = CalculateCurrentDamage();
            damageText.text = $"����: {currentDamage}";
        }
    }

    // ������ �������� ����� � ������ ������ ������
    private int CalculateCurrentDamage()
    {
        if (abilityData == null || playerData == null)
            return (int)(abilityData != null ? abilityData.baseDamage : 0);

        int elementLevel = GetElementLevel(abilityData.element);
        float multiplier = GetDamageMultiplier(elementLevel);

        return Mathf.RoundToInt(abilityData.baseDamage * multiplier);
    }

    private int GetElementLevel(ElementType element)
    {
        switch (element)
        {
            case ElementType.Fire: return playerData.elementalStats.fireLevel;
            case ElementType.Water: return playerData.elementalStats.waterLevel;
            case ElementType.Earth: return playerData.elementalStats.earthLevel;
            case ElementType.Wind: return playerData.elementalStats.windLevel;
            default: return 1;
        }
    }

    private float GetDamageMultiplier(int elementLevel)
    {
        // ������� ��������� ����� � ����������� �� ������ ������
        // ��������: ������� 1 = 100%, ������� 2 = 130%, ������� 3 = 160% � �.�.
        return 1f + (elementLevel) * 0.4f;
    }

    // ���������� ����� UI
    private void UpdateUI()
    {
        if (abilityData == null) return;

        if (abilityIcon != null && abilityData.icon != null)
            abilityIcon.sprite = abilityData.icon;
        if (abilityNameText != null)
            abilityNameText.text = abilityData.abilityName;
        if (elementText != null)
            elementText.text = abilityData.element.ToString();

        UpdateDamageDisplay(); // ���������� ����� ����� ��� ����������� �����
    }

    void OnClick()
    {
        if (abilityData != null)
        {
            selectionSystem.OnAbilityClicked(abilityData, isSlot);
        }
    }
}