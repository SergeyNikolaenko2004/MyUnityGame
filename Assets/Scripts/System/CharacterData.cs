using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite icon;
    public GameObject characterPrefab;
    public AbilityData[] abilities;

    [Header("Elemental Stats")]
    public ElementalStats elementalStats;

    [System.Serializable]
    public class ElementalStats
    {
        [Range(0, 5)] public int fireLevel = 0;
        [Range(0, 5)] public int waterLevel = 0;
        [Range(0, 5)] public int earthLevel = 0;
        [Range(0, 5)] public int windLevel = 0;

        public int GetElementLevel(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => fireLevel,
                ElementType.Water => waterLevel,
                ElementType.Earth => earthLevel,
                ElementType.Wind => windLevel,
                _ => 0
            };
        }

        public void UpgradeElement(ElementType element, int points = 1)
        {
            switch (element)
            {
                case ElementType.Fire:
                    fireLevel = Mathf.Min(5, fireLevel + points);
                    break;
                case ElementType.Water:
                    waterLevel = Mathf.Min(5, waterLevel + points);
                    break;
                case ElementType.Earth:
                    earthLevel = Mathf.Min(5, earthLevel + points);
                    break;
                case ElementType.Wind:
                    windLevel = Mathf.Min(5, windLevel + points);
                    break;
            }
        }

        public float GetElementDamageMultiplier(ElementType element)
        {
            int level = GetElementLevel(element);
            return 1f + level * 0.4f;
        }
    }
}