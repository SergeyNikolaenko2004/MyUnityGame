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

    public void Initialize(AbilityData data, AbilitySelectionSystem system, bool isSlotButton)
    {
        abilityData = data;
        selectionSystem = system;
        isSlot = isSlotButton;

        if (abilityIcon != null && data.icon != null)
            abilityIcon.sprite = data.icon;
        if (abilityNameText != null) abilityNameText.text = data.abilityName;
        if (elementText != null) elementText.text = data.element.ToString();
        if (damageText != null) damageText.text = $"Урон: {data.baseDamage}";

        if (button != null) button.onClick.AddListener(OnClick);
        if (selectedIndicator != null) selectedIndicator.SetActive(false);
    }

    public void InitializeAsSlot(int index, AbilitySelectionSystem system)
    {
        isSlot = true;
        slotIndex = index;
        selectionSystem = system;

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
        if (abilityIcon != null && data.icon != null)
            abilityIcon.sprite = data.icon;
        if (abilityNameText != null) abilityNameText.text = data.abilityName;
        if (elementText != null) elementText.text = data.element.ToString();
        if (damageText != null) damageText.text = $"Урон: {data.baseDamage}";
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

    void OnClick()
    {
        if (abilityData != null)
        {
            selectionSystem.OnAbilityClicked(abilityData, isSlot);
        }
    }
}