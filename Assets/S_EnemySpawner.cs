using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject enemyPrefab;
        public int dangerValue = 1; // Default danger value, to be replaced by ScriptableObject later
    }

    public List<EnemyType> enemyTypes = new List<EnemyType>();
    public int dangerBudget = 10;
    public float spawnRadius = 10f;
    public int maxSpawnAttempts = 30;

    private void Start()
    {
        // Spawn enemies immediately when the script starts
        SpawnEnemies();
    }

    public void SpawnEnemies()
    {
        int remainingBudget = dangerBudget;

        while (remainingBudget > 0 && enemyTypes.Count > 0)
        {
            EnemyType selectedEnemy = enemyTypes[Random.Range(0, enemyTypes.Count)];

            if (selectedEnemy.dangerValue <= remainingBudget)
            {
                if (SpawnEnemy(selectedEnemy))
                {
                    remainingBudget -= selectedEnemy.dangerValue;
                }
            }
            else
            {
                // If we can't afford any more enemies, break the loop
                break;
            }
        }

        Debug.Log($"Spawning complete. Remaining budget: {remainingBudget}");
    }

    private bool SpawnEnemy(EnemyType enemyType)
    {
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, spawnRadius, NavMesh.AllAreas))
            {
                Instantiate(enemyType.enemyPrefab, hit.position, Quaternion.identity);
                return true;
            }
        }

        Debug.LogWarning("Failed to find a valid spawn point after " + maxSpawnAttempts + " attempts.");
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Draw some sample points on the NavMesh
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, spawnRadius, NavMesh.AllAreas))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.position, 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(randomPoint, 0.5f);
            }
        }
    }
}