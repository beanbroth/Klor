
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
            // Get the hit position
            Vector3 hitPosition = collision.contacts[0].point;

            // Apply damage with hit location
            health.TakeDamage(damage, hitPosition);

            // Spawn hit effect
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
            }
        }
        else
        {
            // Spawn hit effect for non-damageable objects (e.g., walls)
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, collision.contacts[0].point, Quaternion.identity);
            }
        }

        // Destroy the bullet
        Destroy(gameObject);
    }
}
