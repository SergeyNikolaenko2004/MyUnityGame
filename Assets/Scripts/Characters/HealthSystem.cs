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
    public Image currentHealthBar;    
    public Image currentHealthGlobe; 
    public TMP_Text healthText;     

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
        UpdateGraphics();
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateGraphics();

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
        UpdateGraphics(); 
    }
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
    IEnumerator PlayerHurts()
    {
        yield return null;
    }

    IEnumerator PlayerDied()
    {
        yield return null;
        Destroy(gameObject);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateGraphics();
    }

    private void OnValidate()
    {
        UpdateGraphics();
    }
}