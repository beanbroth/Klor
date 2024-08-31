using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_BulletController : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffectPrefab;

    private void Start()
    {
        // Destroy the bullet after its lifetime
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object has a BaseHealth component
        if (collision.transform.root.TryGetComponent(out BaseHealth health))
        {
            // Apply damage
            health.TakeDamage(damage);

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // Spawn hit effect for non-damageable objects (e.g., walls)
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            }
        }

        // Destroy the bullet
        Destroy(gameObject);
    }
}