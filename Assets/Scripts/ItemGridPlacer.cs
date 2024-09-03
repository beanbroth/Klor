using UnityEngine;
using UnityEditor;

public class ItemGridPlacer : MonoBehaviour
{
    public LootTable lootTable;
    public float gridSpacing = 1f;
    public int gridSize = 5;
    public float spawnHeight = 1f;
    public WorldItem worldItemPrefab;

    public void PlaceAllItemsInGrid()
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

        int itemsPerRow = Mathf.CeilToInt(Mathf.Sqrt(lootTable.allItems.Count));
        Vector3 startPosition = transform.position - new Vector3(gridSize * gridSpacing / 2, 0, gridSize * gridSpacing / 2);

        for (int i = 0; i < lootTable.allItems.Count; i++)
        {
            BaseItemData item = lootTable.allItems[i];
            int row = i / itemsPerRow;
            int col = i % itemsPerRow;

            Vector3 placePosition = startPosition + new Vector3(col * gridSpacing, spawnHeight, row * gridSpacing);

            // Instantiate the prefab
            WorldItem itemObject = Instantiate(worldItemPrefab, placePosition, Quaternion.identity);
            itemObject.name = item.ItemName;
            itemObject.itemData = item;
            
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ItemGridPlacer))]
public class ItemGridPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ItemGridPlacer gridPlacer = (ItemGridPlacer)target;
        if (GUILayout.Button("Place All Items in Grid"))
        {
            gridPlacer.PlaceAllItemsInGrid();
        }
    }
}
#endif