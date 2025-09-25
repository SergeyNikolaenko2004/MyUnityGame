using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaterWaveAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData abilityData;
    public GameObject waterWavePrefab;
    public SpriteRenderer playerSprite;
    public Transform castPoint;
    public Animator animator;

    [Header("Wave Settings")]
    public float waveSpeed = 10f;
    public float waveDistance = 10f;
    public float knockbackForce = 5f;
    public float castAnimationDelay = 0.3f;

    [Header("Control Settings")]
    public KeyCode currentCastKey = KeyCode.None;
    private float lastCastTime = 0f;
    private bool isCasting = false;
    private int abilityIndex = -1;
    private bool isAbilityReady = true;

    // Новые переменные для системы комбо
    private float currentCooldownRemaining = 0f;
    private Coroutine cooldownCoroutine;

    private float originalCooldown;

    void Start()
    {
        originalCooldown = abilityData.cooldown;
        lastCastTime = -originalCooldown;
        UpdateAbilityState();
    }

    void Update()
    {
        if (currentCastKey == KeyCode.None) return;

        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.CurrentState != GameStateManager.GameState.Playing)
            return;

        CheckAbilityReady();

        if (Input.GetKeyDown(currentCastKey) && isAbilityReady && !isCasting)
        {
            StartCast();
        }
    }

    void CheckAbilityReady()
    {
        isAbilityReady = currentCooldownRemaining <= 0f && !isCasting;
    }

    // Новые методы для системы комбо
    public void ReduceCooldown(float reductionMultiplier)
    {
        if (cooldownCoroutine != null && currentCooldownRemaining > 0)
        {
            StopCoroutine(cooldownCoroutine);
            currentCooldownRemaining *= reductionMultiplier;
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(currentCooldownRemaining));

            Debug.Log($"Сокращено КД способности {abilityData.abilityName} до {currentCooldownRemaining:F1} сек");
        }
    }

    public float GetCurrentCooldown()
    {
        return currentCooldownRemaining;
    }

    public float GetCooldownRemaining()
    {
        return Mathf.Max(0f, currentCooldownRemaining);
    }

    public float GetCooldownTotal()
    {
        return abilityData.cooldown;
    }

    public AbilityData GetAbilityData()
    {
        return abilityData;
    }

    public int GetAbilityIndex()
    {
        return abilityIndex;
    }

    public bool IsAbilityReady()
    {
        return isAbilityReady;
    }

    public void UpdateAbilityState()
    {
        if (characterData != null && characterData.abilities != null && abilityData != null)
        {
            abilityIndex = -1;

            for (int i = 0; i < characterData.abilities.Length; i++)
            {
                if (characterData.abilities[i] == abilityData)
                {
                    abilityIndex = i;
                    break;
                }
            }

            if (abilityIndex >= 0 && abilityIndex < 4)
            {
                currentCastKey = InputSettingsSystem.GetAbilityKey(abilityIndex);
                Debug.Log($"Способность {abilityData.abilityName} назначена на кнопку: {currentCastKey}");
            }
            else
            {
                currentCastKey = KeyCode.None;
                Debug.Log($"Способность {abilityData.abilityName} не выбрана");
            }
        }
        else
        {
            currentCastKey = KeyCode.None;
        }
    }

    void StartCast()
    {
        isCasting = true;
        lastCastTime = Time.time;
        isAbilityReady = false;

        animator.SetTrigger("WaterWaveAttack");
        StartCoroutine(CastWithDelay(castAnimationDelay));
    }

    IEnumerator CastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CreateWaterWave();

        // Запускаем перезарядку с учетом комбо
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        float reducedCooldown = originalCooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(reducedCooldown));
    }

    IEnumerator CooldownCoroutine(float customCooldown = -1f)
    {
        float cooldownTime = customCooldown > 0 ? customCooldown :
            originalCooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();

        currentCooldownRemaining = cooldownTime;
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            currentCooldownRemaining = cooldownTime - elapsedTime;
            yield return null;
        }

        isAbilityReady = true;
        isCasting = false;
        currentCooldownRemaining = 0f;
        Debug.Log($"Способность {abilityData.abilityName} готова к использованию");
    }

    void CreateWaterWave()
    {
        bool isFacingLeft = playerSprite.flipX;
        float direction = isFacingLeft ? -1f : 1f;

        Vector3 spawnPosition = castPoint.position;

        GameObject wave = Instantiate(waterWavePrefab, spawnPosition, Quaternion.identity);

        Vector3 scale = wave.transform.localScale;
        wave.transform.localScale = new Vector3(
            scale.x * direction,
            scale.y,
            scale.z
        );

        WaterWaveController waveController = wave.GetComponent<WaterWaveController>();
        if (waveController == null) waveController = wave.AddComponent<WaterWaveController>();

        float modifiedDamage = abilityData.GetModifiedDamage(characterData);
        // Убрали передачу parentAbility - теперь не нужно
        waveController.Initialize(modifiedDamage, direction, waveSpeed, waveDistance, knockbackForce);
    }

    public bool IsAbilitySelected()
    {
        return abilityIndex >= 0 && abilityIndex < 4;
    }

    public KeyCode GetCurrentCastKey()
    {
        return currentCastKey;
    }

    public void SetCastKey(KeyCode newKey)
    {
        currentCastKey = newKey;
        PlayerPrefs.SetString("WaterWaveKey", newKey.ToString());
        PlayerPrefs.Save();
    }
}
public class WaterWaveController : MonoBehaviour
{
    private float damage;
    private float direction;
    private float speed;
    private float maxDistance;
    private float knockback;
    private Vector3 startPosition;
    private HashSet<Collider2D> damagedEnemies = new HashSet<Collider2D>();
    private int hitsCount = 0;

    public void Initialize(float damageValue, float dir, float spd, float distance, float knockbackForce)
    {
        damage = damageValue;
        direction = dir;
        speed = spd;
        maxDistance = distance;
        knockback = knockbackForce;
        startPosition = transform.position;

        SetupCollider();
        StartCoroutine(MoveWave());
    }

    void SetupCollider()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = new Vector2(collider.size.x, collider.size.y);
            collider.isTrigger = true;
        }
    }

    IEnumerator MoveWave()
    {
        float traveledDistance = 0f;

        while (traveledDistance < maxDistance)
        {
            float moveDistance = speed * Time.deltaTime;
            transform.Translate(Vector3.right * direction * moveDistance);
            traveledDistance += moveDistance;
            yield return null;
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !damagedEnemies.Contains(other))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);
                damagedEnemies.Add(other);
                ApplyKnockback(other);
                hitsCount++;

                // ТОЛЬКО добавляем комбо - ComboSystem сам применит сокращение
                ComboSystem.Instance?.AddCombo(1);
            }
        }
    }

    void ApplyKnockback(Collider2D enemy)
    {
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 knockbackDirection = new Vector2(direction, 0.3f).normalized;
            rb.AddForce(knockbackDirection * knockback, ForceMode2D.Impulse);
        }
    }
    void OnDrawGizmosSelected()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            Gizmos.DrawWireCube(transform.position, new Vector3(collider.size.x, collider.size.y, 0f));
        }
    }
}