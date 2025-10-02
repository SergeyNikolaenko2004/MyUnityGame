using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance;

    [Header("Combo Settings")]
    public int currentCombo = 0;
    public float comboResetTime = 3f;
    public float comboTimeRemaining = 0f;

    [Header("Cooldown Reduction Settings")]
    public AnimationCurve cdReductionCurve;
    public float maxCDReduction = 0.5f;
    public int maxComboForMaxReduction = 20;

    [Header("Debug")]
    [SerializeField] private float currentReductionMultiplier = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (comboTimeRemaining > 0)
        {
            comboTimeRemaining -= Time.deltaTime;
            if (comboTimeRemaining <= 0)
            {
                ResetCombo();
            }
        }

        currentReductionMultiplier = GetCooldownReductionMultiplier();
    }

    public void AddCombo(int amount = 1)
    {
        currentCombo += amount;
        comboTimeRemaining = comboResetTime;

        UIManager.Instance?.UpdateComboCounter(currentCombo);
        UIManager.Instance?.UpdateCDReduction(GetCooldownReductionMultiplier());

        ApplyCooldownReductionToAllAbilities();
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        UIManager.Instance?.UpdateComboCounter(0);
        UIManager.Instance?.UpdateCDReduction(1f);
    }

    public float GetCooldownReductionMultiplier()
    {
        if (currentCombo <= 0) return 1f;

        float normalizedCombo = Mathf.Clamp01((float)currentCombo / maxComboForMaxReduction);
        float reductionPercentage = cdReductionCurve.Evaluate(normalizedCombo) * maxCDReduction;

        return 1f - reductionPercentage;
    }

    private void ApplyCooldownReductionToAllAbilities()
    {
        float reductionMultiplier = GetCooldownReductionMultiplier();

        IAbility[] abilities = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IAbility>()
            .ToArray();

        foreach (IAbility ability in abilities)
        {
            ability.ReduceCooldown(reductionMultiplier);
        }
    }

    void OnGUI()
    {
        GUILayout.Label($"Combo: {currentCombo}");
        GUILayout.Label($"CD Multiplier: {currentReductionMultiplier:F2}");
        GUILayout.Label($"Reduction: {(1 - currentReductionMultiplier) * 100:F1}%");
    }
}