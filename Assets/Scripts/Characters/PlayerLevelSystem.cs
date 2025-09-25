using UnityEngine;
using UnityEngine.UI;

public class PlayerLevelSystem : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int baseExpRequired = 100;
    [SerializeField] private float expMultiplier = 1.2f;

    [Header("Skill Points")]
    [SerializeField] private int availableSkillPoints = 0;
    [SerializeField] private int skillPointsPerLevel = 1;

    [Header("Experience UI")]
    [SerializeField] private Slider expSlider;
    [SerializeField] private Text levelText;
    [SerializeField] private Text expText; 
    [SerializeField] private GameObject levelUpEffect; 

    [Header("Inventory Reference")]
    [SerializeField] private InventorySystem inventorySystem;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = ExpToNextLevel;
            expSlider.value = currentExp;
        }

        UpdateLevelUI();
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        UpdateExpUI();
        Debug.Log($"Получено {amount} опыта. Всего: {currentExp}/{ExpToNextLevel}");

        while (currentExp >= ExpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        currentExp -= ExpToNextLevel;
        currentLevel++;
        availableSkillPoints += skillPointsPerLevel;

        if (levelUpEffect != null)
        {
            Instantiate(levelUpEffect, transform.position, Quaternion.identity);
        }

        UpdateLevelUI();

        if (inventorySystem != null && inventorySystem.isInventoryOpen)
        {
            inventorySystem.UpdateStatsUI();
        }

        Debug.Log($"Уровень UP! Теперь {currentLevel}. Получено {skillPointsPerLevel} очков. Всего очков: {availableSkillPoints}");
    }

    private void UpdateExpUI()
    {
        if (expSlider != null)
        {
            expSlider.maxValue = ExpToNextLevel;
            expSlider.value = currentExp;
        }

        if (expText != null)
        {
            expText.text = $"{currentExp} / {ExpToNextLevel}";
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"Уровень: {currentLevel}";
        }

        UpdateExpUI();
    }

    public bool UseSkillPoint(int amount = 1)
    {
        if (availableSkillPoints >= amount)
        {
            availableSkillPoints -= amount;
            Debug.Log($"Использовано {amount} очков. Осталось: {availableSkillPoints}");
            return true;
        }
        Debug.LogWarning("Недостаточно очков уровней!");
        return false;
    }

    private int GetExpToNextLevel()
    {
        return Mathf.RoundToInt(baseExpRequired * Mathf.Pow(expMultiplier, currentLevel - 1));
    }

    public int CurrentLevel => currentLevel;
    public int CurrentExp => currentExp;
    public int ExpToNextLevel => GetExpToNextLevel();
    public int AvailableSkillPoints => availableSkillPoints;
}