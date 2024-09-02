using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_BulletController : MonoBehaviour
{
    [SerializeField] public float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float trailLingeringTime = 0.5f; // Time for the trail to linger after hit

    [SerializeField]  private Rigidbody rb;
    [SerializeField] private TrailRenderer trailRenderer;

    private void Start()
    {
        trailLingeringTime = trailRenderer.time;
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

        // Disable the bullet's renderer and collider
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Stop the bullet's movement
        rb.velocity = Vector3.zero;
        rb.isKinematic = true;

        // Keep the trail renderer active
        if (trailRenderer != null)
        {
            trailRenderer.transform.SetParent(null);
            Destroy(trailRenderer.gameObject, trailLingeringTime);
        }

        // Destroy the bullet object after the trail has disappeared
        Destroy(gameObject, trailLingeringTime);
    }
}