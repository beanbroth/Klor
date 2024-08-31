using UnityEngine;
using System.Collections;

public class S_SpriteDamageController : MonoBehaviour
{
    private int enemyIndex;
    private MaterialPropertyBlock propertyBlock;
    private Renderer rendererComponent;
    private BaseHealth healthComponent;

    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color flashColor = Color.red;

    void Start()
    {
        enemyIndex = S_DamageManager.Instance.GetUniqueEnemyIndex();
        rendererComponent = GetComponent<Renderer>();
        healthComponent = GetComponentInParent<BaseHealth>();

        if (healthComponent == null)
        {
            Debug.LogError("BaseHealth component not found on this GameObject!");
            return;
        }

        // Subscribe to events
        healthComponent.OnDamageTaken += HandleDamageTaken;
        healthComponent.OnDeath += HandleDeath;

        propertyBlock = new MaterialPropertyBlock();
        rendererComponent.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_EnemyID", enemyIndex);
        rendererComponent.SetPropertyBlock(propertyBlock);

        // Set initial flash color and intensity
        propertyBlock.SetColor("_FlashColor", flashColor);
        propertyBlock.SetFloat("_FlashIntensity", 0f);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }

    private void HandleDamageTaken(float damage)
    {
        // Trigger visual effects for damage
        Vector2 randomPosition = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        float randomRotation = Random.Range(0f, 2f * Mathf.PI);
        S_DamageManager.Instance.TakeDamage(enemyIndex, damage, 0, randomPosition, randomRotation);

        // Trigger flash effect
        StartCoroutine(FlashRoutine());
    }

    private void HandleDeath()
    {
        // Handle death effects if needed
        Debug.Log("Enemy died!");
    }

    private IEnumerator FlashRoutine()
    {
        float elapsedTime = 0f;
        while (elapsedTime < flashDuration)
        {
            float intensity = Mathf.PingPong(elapsedTime / flashDuration * 2, 1f);
            propertyBlock.SetFloat("_FlashIntensity", 1);
            rendererComponent.SetPropertyBlock(propertyBlock);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure flash is turned off at the end
        propertyBlock.SetFloat("_FlashIntensity", 0f);
        rendererComponent.SetPropertyBlock(propertyBlock);
    }

    void OnDestroy()
    {
        if (healthComponent != null)
        {
            // Unsubscribe from events
            healthComponent.OnDamageTaken -= HandleDamageTaken;
            healthComponent.OnDeath -= HandleDeath;
        }

        S_DamageManager.Instance.ReleaseEnemyIndex(enemyIndex);
    }
}