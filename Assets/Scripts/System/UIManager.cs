using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Combo UI")]
    public TMP_Text comboText;
    public Image comboMeter;
    public Animator comboAnimator;

    [Header("Cooldown Reduction UI")]
    public TMP_Text cdReductionText; // Добавьте этот элемент в UI
    public Image cdReductionMeter;   // Добавьте этот элемент в UI

    void Awake()
    {
        Instance = this;
    }

    public void UpdateComboCounter(int comboCount)
    {
        if (comboText != null)
            comboText.text = $"{comboCount}";

        if (comboMeter != null)
            comboMeter.fillAmount = (float)comboCount / ComboSystem.Instance.maxComboForMaxReduction;

        if (comboAnimator != null && comboCount > 0)
            comboAnimator.SetTrigger("ComboAdded");
    }

    // ДОБАВЬТЕ ЭТОТ МЕТОД
    public void UpdateCDReduction(float reductionMultiplier)
    {
        // Если у вас есть текстовое поле для отображения сокращения КД
        if (cdReductionText != null)
        {
            float reductionPercentage = (1 - reductionMultiplier) * 100;
            cdReductionText.text = $"-{reductionPercentage:F1}% CD";
        }

        // Если у вас есть полоска для визуализации сокращения КД
        if (cdReductionMeter != null)
        {
            cdReductionMeter.fillAmount = 1 - reductionMultiplier;
        }
    }
}