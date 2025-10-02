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
    public TMP_Text cdReductionText;
    public Image cdReductionMeter;

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

    public void UpdateCDReduction(float reductionMultiplier)
    {
        if (cdReductionText != null)
        {
            float reductionPercentage = (1 - reductionMultiplier) * 100;
            cdReductionText.text = $"-{reductionPercentage:F1}% CD";
        }

        if (cdReductionMeter != null)
        {
            cdReductionMeter.fillAmount = 1 - reductionMultiplier;
        }
    }
}