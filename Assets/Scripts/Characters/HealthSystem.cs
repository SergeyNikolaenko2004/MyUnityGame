using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

[DisallowMultipleComponent]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    [Header("Health UI (�� ������ ����)")]
    public Image currentHealthBar;    // Health Bar
    public Image currentHealthGlobe;  // Health Globe  
    public TMP_Text healthText;       // Health Text

    [Header("Damage Text Settings")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.5f, 0);

    protected virtual void Awake()
    {
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        InitializeHealthUI();
    }

    private void InitializeHealthUI()
    {
        // ������������� �� ������ ����
        UpdateGraphics();
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateGraphics(); // ��������� ������� �� ������ ����

        Debug.Log($"{name} ������� {damage} �����. �������� HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(PlayerHurts());
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} ���������!");
        StartCoroutine(PlayerDied());
    }

    public void SetHealth(float newMaxHealth, bool resetCurrent = true)
    {
        maxHealth = newMaxHealth;
        if (resetCurrent) currentHealth = maxHealth;
        UpdateGraphics(); // ��������� ������� �� ������ ����
    }

    //==============================================================
    // ������� �� ������ ���� ��� ������ � Health Bar � Globe
    //==============================================================
    private void UpdateHealthBarUI()
    {
        if (currentHealthBar != null)
        {
            float ratio = currentHealth / maxHealth;
            currentHealthBar.rectTransform.localPosition = new Vector3(
                currentHealthBar.rectTransform.rect.width * ratio - currentHealthBar.rectTransform.rect.width, 0, 0);
        }
    }

    private void UpdateHealthGlobeUI()
    {
        if (currentHealthGlobe != null)
        {
            float ratio = currentHealth / maxHealth;
            currentHealthGlobe.rectTransform.localPosition = new Vector3(
                0, currentHealthGlobe.rectTransform.rect.height * ratio - currentHealthGlobe.rectTransform.rect.height, 0);
        }
    }

    private void UpdateHealthTextUI()
    {
        if (healthText != null)
        {
            healthText.text = currentHealth.ToString("0") + "/" + maxHealth.ToString("0");
        }
    }

    private void UpdateGraphics()
    {
        UpdateHealthBarUI();
        UpdateHealthGlobeUI();
        UpdateHealthTextUI();
    }

    //==============================================================
    // Coroutine �� ������ ����
    //==============================================================
    IEnumerator PlayerHurts()
    {
        // Player gets hurt. Do stuff.. play anim, sound..
        // PopupText.Instance.Popup("Ouch!", 1f, 1f); // ���������������� ���� �����

        yield return null;
    }

    IEnumerator PlayerDied()
    {
        // Player is dead. Do stuff.. play anim, sound..
        // PopupText.Instance.Popup("You have died!", 1f, 1f); // ���������������� ���� �����

        yield return null;
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateGraphics(); // ��������� ������� �� ������ ����
    }

    private void OnValidate()
    {
        // ��������� ��� ������ UI
        UpdateGraphics();
    }
}