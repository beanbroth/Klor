using UnityEngine;
using System.Collections.Generic;

public class S_DamageManager : MonoBehaviour
{
    public static S_DamageManager Instance { get; private set; }

    public struct DamageInfo
    {
        public Vector4 positionAndType; // xy = position, z = damage type, w = rotation
        public float scale;
        public float damage;
    }

    public int maxEnemies = 100;
    public int maxDamageInstancesPerEnemy = 10;
    private ComputeBuffer damageBuffer;
    private DamageInfo[] damageData;
    private HashSet<int> usedIndices = new HashSet<int>();
    public Material sharedMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeDamageBuffer();
    }

    void InitializeDamageBuffer()
    {
        damageData = new DamageInfo[maxEnemies * maxDamageInstancesPerEnemy];
        damageBuffer = new ComputeBuffer(maxEnemies * maxDamageInstancesPerEnemy, sizeof(float) * 6);

        for (int i = 0; i < damageData.Length; i++)
        {
            damageData[i] = new DamageInfo
            {
                positionAndType = new Vector4(0, 0, 0, 0),
                scale = 0,
                damage = 0
            };
        }

        UpdateDamageBuffer();
        sharedMaterial.SetBuffer("_DamageBuffer", damageBuffer);
        sharedMaterial.SetInt("_MaxDamageInstancesPerEnemy", maxDamageInstancesPerEnemy);
    }

    void UpdateDamageBuffer()
    {
        damageBuffer.SetData(damageData);
    }

    public int GetUniqueEnemyIndex()
    {
        for (int i = 0; i < maxEnemies; i++)
        {
            if (!usedIndices.Contains(i))
            {
                usedIndices.Add(i);
                return i;
            }
        }
        Debug.LogError("No more enemy indices available!");
        return -1;
    }

    public void ReleaseEnemyIndex(int index)
    {
        usedIndices.Remove(index);
        // Clear damage data for this enemy
        for (int i = 0; i < maxDamageInstancesPerEnemy; i++)
        {
            int dataIndex = index * maxDamageInstancesPerEnemy + i;
            damageData[dataIndex] = new DamageInfo();
        }
        UpdateDamageBuffer();
    }

    public void TakeDamage(int enemyIndex, float damageAmount, int damageType, Vector2 position, float rotation)
    {
        if (enemyIndex < 0 || enemyIndex >= maxEnemies) return;

        int startIndex = enemyIndex * maxDamageInstancesPerEnemy;
        for (int i = 0; i < maxDamageInstancesPerEnemy; i++)
        {
            int dataIndex = startIndex + i;
            if (damageData[dataIndex].damage == 0)
            {
                damageData[dataIndex] = new DamageInfo
                {
                    positionAndType = new Vector4(position.x, position.y, damageType, rotation),
                    scale = 0.2f, // You might want to make this configurable
                    damage = damageAmount
                };
                break;
            }
        }

        UpdateDamageBuffer();
    }

    void OnDisable()
    {
        if (damageBuffer != null)
        {
            damageBuffer.Release();
            damageBuffer = null;
        }
    }
}