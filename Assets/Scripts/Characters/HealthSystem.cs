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

    [Header("Health UI (из чужого кода)")]
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
        // Инициализация из чужого кода
        UpdateGraphics();
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateGraphics(); // Обновляем графику из чужого кода

        Debug.Log($"{name} получил {damage} урона. Осталось HP: {currentHealth}");

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
        Debug.Log($"{name} уничтожен!");
        StartCoroutine(PlayerDied());
    }

    public void SetHealth(float newMaxHealth, bool resetCurrent = true)
    {
        maxHealth = newMaxHealth;
        if (resetCurrent) currentHealth = maxHealth;
        UpdateGraphics(); // Обновляем графику из чужого кода
    }

    //==============================================================
    // Функции из чужого кода для работы с Health Bar и Globe
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
    // Coroutine из чужого кода
    //==============================================================
    IEnumerator PlayerHurts()
    {
        // Player gets hurt. Do stuff.. play anim, sound..
        // PopupText.Instance.Popup("Ouch!", 1f, 1f); // Раскомментируйте если нужно

        yield return null;
    }

    IEnumerator PlayerDied()
    {
        // Player is dead. Do stuff.. play anim, sound..
        // PopupText.Instance.Popup("You have died!", 1f, 1f); // Раскомментируйте если нужно

        yield return null;
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateGraphics(); // Обновляем графику из чужого кода
    }

    private void OnValidate()
    {
        // Валидация для нового UI
        UpdateGraphics();
    }
}