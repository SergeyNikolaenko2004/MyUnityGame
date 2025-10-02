using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlizzardAttack : MonoBehaviour, IAbility
{
    [Header("Required References")]
    public CharacterData characterData;
    public AbilityData abilityData;
    public GameObject freezeEffectPrefab; 
    public Animator animator;

    [Header("Area Attack Settings")]
    public float attackRadius = 5f;
    public float freezeEffectDuration = 2f;
    public float castAnimationDelay = 0.3f;

    [Header("Control Settings")]
    public KeyCode currentCastKey = KeyCode.None;
    private float lastCastTime = 0f;
    private bool isCasting = false;
    private int abilityIndex = -1;
    private bool isAbilityReady = true;

    private float currentCooldownRemaining = 0f;
    private Coroutine cooldownCoroutine;

    void Start()
    {
        lastCastTime = -abilityData.cooldown;
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

        animator.SetTrigger("blizzard");
        StartCoroutine(CastWithDelay(castAnimationDelay));
    }

    IEnumerator CastWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CastBlizzard();

        if (cooldownCoroutine != null)
            StopCoroutine(cooldownCoroutine);
        cooldownCoroutine = StartCoroutine(CooldownCoroutine());
    }

    IEnumerator CooldownCoroutine(float customCooldown = -1f)
    {
        float cooldownTime = customCooldown > 0 ? customCooldown : abilityData.cooldown;
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

    void CastBlizzard()
    {
        int hitCount = DealAreaDamage();
        CreateFreezeEffect();

    }

    int DealAreaDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRadius);
        float modifiedDamage = abilityData.GetModifiedDamage(characterData);
        int hitCount = 0;

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                HealthSystem health = enemy.GetComponent<HealthSystem>();
                if (health != null)
                {
                    health.TakeDamage(modifiedDamage);
                    hitCount++;
                    ComboSystem.Instance?.AddCombo(1);
                }
            }
        }
        return hitCount;
    }

    void CreateFreezeEffect()
    {
        if (freezeEffectPrefab != null)
        {
            GameObject freezeEffect = Instantiate(freezeEffectPrefab);
            freezeEffect.transform.SetParent(FindAnyObjectByType<Canvas>().transform, false);

            FreezeEffectController effectController = freezeEffect.GetComponent<FreezeEffectController>();
            if (effectController != null)
            {
                effectController.Initialize(freezeEffectDuration);
            }
            else
            {
                Destroy(freezeEffect, freezeEffectDuration);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
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

public class FreezeEffectController : MonoBehaviour
{
    private float remainingDuration;

    public void Initialize(float duration)
    {
        remainingDuration = duration;
        StartCoroutine(DurationCountdown());
    }

    IEnumerator DurationCountdown()
    {
        yield return new WaitForSeconds(remainingDuration);
        Destroy(gameObject);
    }
}