using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
    public bool useDelayedSpawning = false; // Toggle for delayed spawning
    public float spawnDelay = 0.5f; // Delay between spawns in seconds

    private void Start()
    {
        SpawnEnemies(dangerBudget);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            SpawnEnemies(500);
        }
    }

    public void SpawnEnemies(int db)
    {
        if (useDelayedSpawning)
        {
            StartCoroutine(SpawnEnemiesWithDelay(db));
        }
        else
        {
            SpawnEnemiesImmediate(db);
        }
    }

    private void SpawnEnemiesImmediate(int numEnemies)
    {
        int remainingBudget = numEnemies;

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
    }

    private IEnumerator SpawnEnemiesWithDelay(int db)
    {
        int remainingBudget = db;

        while (remainingBudget > 0 && enemyTypes.Count > 0)
        {
            EnemyType selectedEnemy = enemyTypes[Random.Range(0, enemyTypes.Count)];

            if (selectedEnemy.dangerValue <= remainingBudget)
            {
                if (SpawnEnemy(selectedEnemy))
                {
                    remainingBudget -= selectedEnemy.dangerValue;
                    yield return new WaitForSeconds(spawnDelay);
                }
            }
            else
            {
                // If we can't afford any more enemies, break the loop
                break;
            }
        }
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
}