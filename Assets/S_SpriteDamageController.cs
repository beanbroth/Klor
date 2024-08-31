using UnityEngine;

public class S_SpriteDamageController : MonoBehaviour
{
    private int enemyIndex;
    private MaterialPropertyBlock propertyBlock;

    void Start()
    {
        enemyIndex = S_DamageManager.Instance.GetUniqueEnemyIndex();

        Renderer renderer = GetComponent<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat("_EnemyID", enemyIndex);
        renderer.SetPropertyBlock(propertyBlock);
    }

    public void TakeDamage(float amount, int damageType)
    {
        Vector2 randomPosition = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
        float randomRotation = Random.Range(0f, 2f * Mathf.PI);
        S_DamageManager.Instance.TakeDamage(enemyIndex, amount, damageType, randomPosition, randomRotation);
    }

    void OnDestroy()
    {
        S_DamageManager.Instance.ReleaseEnemyIndex(enemyIndex);
    }
}