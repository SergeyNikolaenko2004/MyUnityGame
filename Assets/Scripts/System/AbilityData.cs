using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityData", menuName = "Game/Ability Data")]
public class AbilityData : ScriptableObject
{
    [Header("Info")]
    public string abilityName;
    public ElementType element;
    public Sprite icon;

    [Header("Stats")]
    public float baseDamage;
    public float cooldown;

    public float GetModifiedDamage(CharacterData characterData)
    {
        if (characterData == null) return baseDamage;
        return baseDamage * characterData.elementalStats.GetElementDamageMultiplier(element);
    }
}