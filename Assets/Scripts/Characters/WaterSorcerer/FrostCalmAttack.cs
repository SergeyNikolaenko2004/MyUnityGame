using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FrostCalmAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData abilityData;
    public GameObject frostcalmPrefab;
    public SpriteRenderer playerSprite;
    public Transform castPoint;
    public Animator animator;
    private float originalCooldown;

    [Header("Wall Settings")]
    public float wallDuration = 3f;
    public float wallDistance = 2f;
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

        animator.SetTrigger("frostCalm");
        StartCoroutine(CastWithDelay(castAnimationDelay));
    }

    IEnumerator CastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CreateWall();

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

    void CreateWall()
    {
        bool isFacingLeft = playerSprite.flipX;
        Vector3 spawnPosition = castPoint.position +
                             (isFacingLeft ? Vector3.left : Vector3.right) * wallDistance;

        GameObject wall = Instantiate(frostcalmPrefab, spawnPosition, Quaternion.identity);

        Vector3 scale = wall.transform.localScale;
        wall.transform.localScale = new Vector3(
            scale.x * (isFacingLeft ? -1 : 1),
            scale.y,
            scale.z
        );

        FrostCalmController wallController = wall.GetComponent<FrostCalmController>();
        if (wallController == null) wallController = wall.AddComponent<FrostCalmController>();

        float modifiedDamage = abilityData.GetModifiedDamage(characterData);
        wallController.Initialize(modifiedDamage, wallDuration);
    }

    // Метод для применения сокращения КД из контроллера стены
    public void ApplyCooldownReductionFromWall(int hitCount)
    {
        if (ComboSystem.Instance != null && hitCount > 0)
        {
            float reductionMultiplier = ComboSystem.Instance.GetCooldownReductionMultiplier();
            ReduceCooldown(reductionMultiplier);
        }
    }

    public bool IsAbilitySelected()
    {
        return abilityIndex >= 0 && abilityIndex < 4;
    }

    public KeyCode GetCurrentCastKey()
    {
        return currentCastKey;
    }
}

public class FrostCalmController : MonoBehaviour
{
    private float damage;
    private float remainingDuration;
    private ParticleSystem[] particles;
    private HashSet<Collider2D> damagedEnemies = new HashSet<Collider2D>();
    private int totalHits = 0;
    private float damageInterval = 0.5f;
    private float lastDamageTime = 0f;

    void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void Initialize(float damageValue, float duration)
    {
        damage = damageValue;
        remainingDuration = duration;

        foreach (var ps in particles)
        {
            ps.Play();
        }

        StartCoroutine(DurationCountdown());
    }

    IEnumerator DurationCountdown()
    {
        while (remainingDuration > 0)
        {
            remainingDuration -= Time.deltaTime;
            yield return null;
        }

        foreach (var ps in particles)
        {
            ps.Stop();
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    void Update()
    {
        // Постоянный урон врагам в зоне
        if (Time.time - lastDamageTime >= damageInterval && damagedEnemies.Count > 0)
        {
            lastDamageTime = Time.time;
            ApplyDamageToEnemiesInZone();
        }
    }

    void ApplyDamageToEnemiesInZone()
    {
        List<Collider2D> enemiesToRemove = new List<Collider2D>();

        foreach (var enemy in damagedEnemies)
        {
            if (enemy == null)
            {
                enemiesToRemove.Add(enemy);
                continue;
            }

            HealthSystem health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage * damageInterval);
                totalHits++;

                // ТОЛЬКО добавляем комбо - ComboSystem сам применит сокращение
                ComboSystem.Instance?.AddCombo(1);
            }
        }

        // Удаляем уничтоженных врагов
        foreach (var enemy in enemiesToRemove)
        {
            damagedEnemies.Remove(enemy);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !damagedEnemies.Contains(other))
        {
            damagedEnemies.Add(other);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (damagedEnemies.Contains(other))
        {
            damagedEnemies.Remove(other);
        }
    }

    // УДАЛИТЕ метод OnDestroy() - он больше не нужен
}