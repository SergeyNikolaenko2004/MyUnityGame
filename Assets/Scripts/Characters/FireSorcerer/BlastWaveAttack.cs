using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlastWaveAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData abilityData;
    public GameObject fireWavePrefab;
    public SpriteRenderer playerSprite;
    public Transform castPoint;
    private Vector3 fixedCastPosition;
    public Animator animator;

    [Header("Wave Settings")]
    public float waveInterval = 0.3f;
    public float waveDistance = 2f;
    public int wavesCount = 3;
    public float castAnimationDelay = 0.3f;
    public float verticalOffset = 0f;

    [Header("Control Settings")]
    public KeyCode currentCastKey = KeyCode.None;
    private float lastCastTime = 0f;
    private bool isCasting = false;
    private int abilityIndex = -1;
    private bool isAbilityReady = true;

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
            fixedCastPosition = castPoint.position;
            StartCast();
        }
    }

    void CheckAbilityReady()
    {
        isAbilityReady = currentCooldownRemaining <= 0;
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
        isAbilityReady = false;
        lastCastTime = Time.time;

        animator.SetTrigger("BlastOfWaveAttack");
        StartCoroutine(CastWithDelay(castAnimationDelay));

        // Запускаем перезарядку
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        float reducedCooldown = originalCooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(reducedCooldown));
    }

    IEnumerator CastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(CastWaveSequence());
    }

    IEnumerator CooldownCoroutine(float cooldownTime)
    {
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

    IEnumerator CastWaveSequence()
    {
        float modifiedDamage = abilityData.GetModifiedDamage(characterData);

        for (int i = 0; i < wavesCount; i++)
        {
            CreateWave(Vector3.left, i * 0.5f, modifiedDamage);
            CreateWave(Vector3.right, i * 0.5f, modifiedDamage);
            yield return new WaitForSeconds(waveInterval);
        }
    }

    void CreateWave(Vector3 direction, float offset, float damage)
    {
        bool isFacingLeft = playerSprite.flipX;
        Vector3 adjustedDirection = isFacingLeft ? new Vector3(-direction.x, direction.y, direction.z) : direction;

        Vector3 spawnPosition = fixedCastPosition + adjustedDirection * (waveDistance + offset);
        spawnPosition.y += verticalOffset;

        GameObject wave = Instantiate(fireWavePrefab, spawnPosition, Quaternion.identity);

        Vector3 scale = wave.transform.localScale;
        wave.transform.localScale = new Vector3(
            scale.x * Mathf.Sign(adjustedDirection.x),
            scale.y,
            scale.z
        );

        BlastWaveController waveController = wave.GetComponent<BlastWaveController>();
        if (waveController == null) waveController = wave.AddComponent<BlastWaveController>();

        waveController.Initialize(damage);
    }

    // Реализация метода из интерфейса IAbility
    public void ReduceCooldown(float reductionMultiplier)
    {
        if (cooldownCoroutine != null && !isAbilityReady)
        {
            StopCoroutine(cooldownCoroutine);

            // Применяем сокращение к оставшемуся времени перезарядки
            currentCooldownRemaining *= reductionMultiplier;

            // Перезапускаем перезарядку с новым временем
            cooldownCoroutine = StartCoroutine(CooldownCoroutine(currentCooldownRemaining));
        }
    }

    public float GetCurrentCooldown()
    {
        return currentCooldownRemaining;
    }

    public void SetVerticalOffset(float newOffset)
    {
        verticalOffset = newOffset;
    }

    public float GetVerticalOffset()
    {
        return verticalOffset;
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
        PlayerPrefs.SetString("BlastWaveKey", newKey.ToString());
        PlayerPrefs.Save();
    }
}

public class BlastWaveController : MonoBehaviour
{
    private float damage;
    private HashSet<Collider2D> damagedEnemies = new HashSet<Collider2D>();

    public void Initialize(float damageValue)
    {
        damage = damageValue;
        SetupPhysics();
        Destroy(gameObject, 1f);
    }

    void SetupPhysics()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
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

                // Добавляем комбо за попадание
                ComboSystem.Instance?.AddCombo(1);
            }
        }
    }

    // УБРАТЬ метод OnDestroy и ApplyCooldownReduction!
    // Сокращение перезарядки теперь обрабатывается в ComboSystem
}