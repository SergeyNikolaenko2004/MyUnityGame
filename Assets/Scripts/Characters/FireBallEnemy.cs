using UnityEngine;

public class FireBallEnemy : MonoBehaviour
{
    [Header("Settings")]
    public float lifetime = 2f;
    public GameObject hitEffect;
    public string enemyTag = "Player";
    public int damage = 10;

    [Header("Animation")]
    public string boomAnimationName = "Boom";

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(enemyTag))
        {
            HealthSystem enemyHealth = collision.gameObject.GetComponent<HealthSystem>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            if (hitEffect != null)
            {
                PlayBoomAnimation(collision.contacts[0].point);
            }
        }

        Destroy(gameObject);
    }

    void PlayBoomAnimation(Vector2 position)
    {
        GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
        Animator anim = effect.GetComponent<Animator>();

    }
}