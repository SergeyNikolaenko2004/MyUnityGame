using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    [Header("Health Bar UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private bool alwaysShowHealthBar = false;
    [SerializeField] private GameObject healthBarCanvas;

    [Header("Damage Text Settings")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Vector3 textOffset = new Vector3(0, 1.5f, 0);

    protected virtual void Awake()
    {
        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }

        InitializeHealthBar();
    }

    private void InitializeHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(alwaysShowHealthBar);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthBar();

        Debug.Log($"{name} получил {damage} урона. Осталось HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{name} уничтожен!");
        Destroy(gameObject);
    }

    public void SetHealth(float newMaxHealth, bool resetCurrent = true)
    {
        maxHealth = newMaxHealth;
        if (resetCurrent) currentHealth = maxHealth;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;

            if (!alwaysShowHealthBar && healthBarCanvas != null)
            {
                healthBarCanvas.SetActive(true);
                CancelInvoke(nameof(HideHealthBar));
                Invoke(nameof(HideHealthBar), 2f);
            }
        }
    }

    private void HideHealthBar()
    {
        if (healthBarCanvas != null && !alwaysShowHealthBar)
        {
            healthBarCanvas.SetActive(false);
        }
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthBar();
    }

    private void OnValidate()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}