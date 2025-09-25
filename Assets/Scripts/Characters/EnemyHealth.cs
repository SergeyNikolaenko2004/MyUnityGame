using UnityEngine;

public class EnemyHealth : HealthSystem
{
    [Header("Experience Reward")]
    [SerializeField] public int expReward = 30;
    [SerializeField] private bool scaleWithLevel = true;
    [SerializeField] private float expScaleFactor = 0.1f;

    protected override void Die() 
    {
        GiveExperience();
        base.Die();
    }

    private void GiveExperience()
    {
        var player = FindFirstObjectByType<PlayerLevelSystem>();
        if (player != null)
        {
            int finalExp = expReward;

            if (scaleWithLevel)
            {
                finalExp = Mathf.RoundToInt(expReward * (1 + player.CurrentLevel * expScaleFactor));
            }

            player.AddExp(finalExp);
            Debug.Log($"Враг убит! Получено {finalExp} опыта");
        }
    }
}