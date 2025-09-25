using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeteorAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData abilityData;
    public GameObject meteorPrefab;
    public string enemyTag = "Enemy";
    public SpriteRenderer playerSprite;
    public Animator animator;

    [Header("Meteor Settings")]
    public float spawnHeight = 10f;
    public float meteorSpeed = 15f;
    public float forwardOffset = 3f;
    public float castAnimationDelay = 0.2f;

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

        if (playerSprite == null) playerSprite = GetComponent<SpriteRenderer>();
        if (animator == null) animator = GetComponent<Animator>();
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
        isAbilityReady = currentCooldownRemaining <= 0f;
    }

    // Реализация методов интерфейса IAbility для системы комбо
    public void ReduceCooldown(float reductionMultiplier)
    {
        if (cooldownCoroutine != null && currentCooldownRemaining > 0)
        {
            StopCoroutine(cooldownCoroutine);
            currentCooldownRemaining *= reductionMultiplier;
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(currentCooldownRemaining));
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
        return originalCooldown;
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
            }
            else
            {
                currentCastKey = KeyCode.None;
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

        animator.SetTrigger("Meteor");
        StartCoroutine(CastWithDelay(castAnimationDelay));
    }

    IEnumerator CastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CreateMeteor();

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
    }

    void CreateMeteor()
    {
        float direction = (playerSprite != null && !playerSprite.flipX) ? 1f : -1f;
        Vector3 spawnPosition = transform.position +
                               new Vector3(forwardOffset * direction, spawnHeight, 0);

        GameObject meteor = Instantiate(meteorPrefab, spawnPosition, Quaternion.identity);
        MeteorProjectile projectile = meteor.GetComponent<MeteorProjectile>();
        if (projectile == null) projectile = meteor.AddComponent<MeteorProjectile>();

        float modifiedDamage = abilityData.GetModifiedDamage(characterData);
        projectile.Initialize(modifiedDamage, enemyTag, direction);
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
        PlayerPrefs.SetString("MeteorKey", newKey.ToString());
        PlayerPrefs.Save();
    }
}

[RequireComponent(typeof(Rigidbody2D))]
public class MeteorProjectile : MonoBehaviour
{
    private float damage;
    private string enemyTag;
    private string groundTag = "Ground";
    private HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();
    private bool hasHitGround = false;
    private SpriteRenderer meteorSprite;

    public void Initialize(float damageValue, string tag, float direction)
    {
        damage = damageValue;
        enemyTag = tag;

        meteorSprite = GetComponent<SpriteRenderer>();
        if (meteorSprite != null)
        {
            meteorSprite.flipX = direction > 0;
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 fallDirection = new Vector2(direction * 0.7f, -1f).normalized;
            rb.linearVelocity = fallDirection * 15f;
        }

        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(groundTag) && !hasHitGround)
        {
            hasHitGround = true;
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag(enemyTag))
        {
            if (!damagedEnemies.Contains(other.gameObject))
            {
                HealthSystem health = other.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(damage);
                    damagedEnemies.Add(other.gameObject);

                    // ДОБАВЛЯЕМ КОМБО ЗА ПОПАДАНИЕ
                    ComboSystem.Instance?.AddCombo(1);
                }
            }
        }
    }
}