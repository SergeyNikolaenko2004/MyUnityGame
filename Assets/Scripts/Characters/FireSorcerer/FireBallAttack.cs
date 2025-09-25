using UnityEngine;
using System.Collections;

public class FireBallAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData fireBallData;
    public GameObject fireBallPrefab;
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
        lastAttackTime = -fireBallData.cooldown;
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
        return fireBallData.cooldown;
    }

    public AbilityData GetAbilityData()
    {
        return fireBallData;
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
        if (characterData != null && characterData.abilities != null && fireBallData != null)
        {
            abilityIndex = -1;

            for (int i = 0; i < characterData.abilities.Length; i++)
            {
                if (characterData.abilities[i] == fireBallData)
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

        animator.SetTrigger("FireBall");
        StartCoroutine(ExecuteAttackAfterAnimation());
    }

    IEnumerator ExecuteAttackAfterAnimation()
    {
        yield return new WaitForSeconds(spawnDelay);
        Attack();

        // Запускаем перезарядку
        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownCoroutine());

        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

    void Attack()
    {
        Vector3 spawnPosition = spawnPoint.position + spawnOffset;
        GameObject fireBall = Instantiate(fireBallPrefab, spawnPosition, spawnPoint.rotation);
        FireBallProjectile projectile = fireBall.GetComponent<FireBallProjectile>();
        if (projectile == null) projectile = fireBall.AddComponent<FireBallProjectile>();

        bool isFacingLeft = playerSprite.flipX;
        float modifiedDamage = fireBallData.GetModifiedDamage(characterData);

        // Убрали передачу parentAbility - теперь не нужно
        projectile.Initialize(modifiedDamage, enemyTag, isFacingLeft);

        if (isFacingLeft)
        {
            SpriteRenderer fireballSprite = fireBall.GetComponent<SpriteRenderer>();
            if (fireballSprite != null) fireballSprite.flipX = true;
        }
    }

    IEnumerator CooldownCoroutine(float customCooldown = -1f)
    {
        // Используем сокращенное время перезарядки
        float cooldownTime = customCooldown > 0 ? customCooldown :
            fireBallData.cooldown * ComboSystem.Instance.GetCooldownReductionMultiplier();

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
}

[RequireComponent(typeof(Rigidbody2D))]
public class FireBallProjectile : MonoBehaviour
{
    private float damage;
    private string enemyTag;
    private Rigidbody2D rb;
    private bool isFacingLeft;
    private FireBallAttack parentAbility;

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
                ComboSystem.Instance?.AddCombo(1); // ТОЛЬКО добавляем комбо
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}