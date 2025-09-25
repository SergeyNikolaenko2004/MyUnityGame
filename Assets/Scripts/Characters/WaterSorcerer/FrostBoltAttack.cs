using UnityEngine;
using System.Collections;

public class FrostBoltAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData frostboltData;
    public GameObject frostboltPrefab;
    public Transform spawnPoint;
    public string enemyTag = "Enemy";
    public SpriteRenderer playerSprite;
    public Animator animator;

    [Header("Spawn Settings")]
    public Vector3 spawnOffset = Vector2.zero;
    public float spawnDelay = 0.3f;

    [Header("Control Settings")]
    public KeyCode currentCastKey = KeyCode.None;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private int abilityIndex = -1;
    private bool isAbilityReady = true;

    // Новые переменные для системы комбо
    private float currentCooldownRemaining = 0f;
    private Coroutine cooldownCoroutine;

    void Start()
    {
        lastAttackTime = -frostboltData.cooldown;
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

        if (Input.GetKeyDown(currentCastKey) && isAbilityReady && !isAttacking)
        {
            StartAttack();
        }
    }

    void CheckAbilityReady()
    {
        isAbilityReady = currentCooldownRemaining <= 0f && !isAttacking;
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
        return frostboltData.cooldown;
    }

    public AbilityData GetAbilityData()
    {
        return frostboltData;
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
        if (characterData != null && characterData.abilities != null && frostboltData != null)
        {
            abilityIndex = -1;

            for (int i = 0; i < characterData.abilities.Length; i++)
            {
                if (characterData.abilities[i] == frostboltData)
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

    void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        isAbilityReady = false;

        animator.SetTrigger("FrostBolt");
        StartCoroutine(ExecuteAttackAfterAnimation());
    }

    IEnumerator ExecuteAttackAfterAnimation()
    {
        yield return new WaitForSeconds(spawnDelay);
        Attack();

        // Запускаем перезарядку с учетом комбо
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);

        float reducedCooldown = frostboltData.cooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();
        cooldownCoroutine = StartCoroutine(CooldownCoroutine(reducedCooldown));

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    IEnumerator CooldownCoroutine(float customCooldown = -1f)
    {
        float cooldownTime = customCooldown > 0 ? customCooldown :
            frostboltData.cooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();

        currentCooldownRemaining = cooldownTime;
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            elapsedTime += Time.deltaTime;
            currentCooldownRemaining = cooldownTime - elapsedTime;
            yield return null;
        }

        isAbilityReady = true;
        currentCooldownRemaining = 0f;
    }

    void Attack()
    {
        Vector3 spawnPosition = spawnPoint.position + spawnOffset;
        GameObject frostBolt = Instantiate(frostboltPrefab, spawnPosition, spawnPoint.rotation);
        FrostBoltProjectile projectile = frostBolt.GetComponent<FrostBoltProjectile>();
        if (projectile == null) projectile = frostBolt.AddComponent<FrostBoltProjectile>();

        bool isFacingLeft = playerSprite.flipX;
        float modifiedDamage = frostboltData.GetModifiedDamage(characterData);

        // Убрали передачу parentAbility - теперь не нужно
        projectile.Initialize(modifiedDamage, enemyTag, isFacingLeft);

        if (isFacingLeft)
        {
            SpriteRenderer frostboltSprite = frostBolt.GetComponent<SpriteRenderer>();
            if (frostboltSprite != null) frostboltSprite.flipX = true;
        }
    }
}

[RequireComponent(typeof(Rigidbody2D))]
public class FrostBoltProjectile : MonoBehaviour
{
    private float damage;
    private string enemyTag;
    private Rigidbody2D rb;
    private bool isFacingLeft;

    public void Initialize(float damageValue, string tag, bool facingLeft)
    {
        damage = damageValue;
        enemyTag = tag;
        isFacingLeft = facingLeft;
        rb = GetComponent<Rigidbody2D>();

        Vector2 direction = isFacingLeft ? Vector2.left : Vector2.right;
        rb.linearVelocity = direction * 20f;

        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(enemyTag))
        {
            HealthSystem health = other.GetComponent<HealthSystem>();
            if (health != null)
            {
                health.TakeDamage(damage);

                // ТОЛЬКО добавляем комбо - ComboSystem сам применит сокращение
                ComboSystem.Instance?.AddCombo(1);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}