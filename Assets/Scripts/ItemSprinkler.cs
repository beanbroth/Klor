using UnityEngine;

public class ItemSprinkler : MonoBehaviour
{
    public LootTable lootTable;
    public float spawnInterval = 1f;
    public float spawnForce = 5f;
    public float spreadAngle = 30f;
    public WorldItem worldItemPrefab;
    private float timer;
    private bool shouldSpawn = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            shouldSpawn = !shouldSpawn;
        }
        if(!shouldSpawn) return;
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnRandomItem();
            timer = 0f;
        }
    }

    private void SpawnRandomItem()
    {
        if (lootTable.allItems.Count == 0)
        {
            Debug.LogWarning("No items in the database!");
            return;
        }

        if (worldItemPrefab == null)
        {
            Debug.LogError("World Item Prefab is not assigned!");
            return;
        }

        BaseItemData randomItem = lootTable.allItems[Random.Range(0, lootTable.allItems.Count)];

        // Instantiate the prefab
        WorldItem itemObject = Instantiate(worldItemPrefab, transform.position, Quaternion.identity);
        itemObject.name = randomItem.ItemName;
        itemObject.itemData = randomItem;
        Rigidbody rb = itemObject.rb;

        // Apply random force within a cone
        Vector3 randomDirection = new Vector3(Random.Range(-spreadAngle, spreadAngle),5,
            Random.Range(-spreadAngle, spreadAngle));
        
        rb.AddForce(randomDirection.normalized * spawnForce, ForceMode.VelocityChange);
        // Setup the WorldItem component
        itemObject.SetupItem();
    }
}