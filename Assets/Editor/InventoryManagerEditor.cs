using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerEditor : Editor
{
    private BaseItemData itemToAdd;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        InventoryManager inventoryManager = (InventoryManager)target;
        EditorGUILayout.Space();
        itemToAdd = (BaseItemData)EditorGUILayout.ObjectField("Item to Add", itemToAdd, typeof(BaseItemData), false);
        if (GUILayout.Button("Create and Add Item"))
        {
            if (itemToAdd != null)
            {
                bool added = inventoryManager.CreateAndAddItem(itemToAdd);
                if (added)
                {
                    Debug.Log($"Item '{itemToAdd.ItemName}' added successfully.");
                }
                else
                {
                    Debug.LogWarning($"Failed to add item '{itemToAdd.ItemName}'. Inventory might be full.");
                }
            }
            else
            {
                Debug.LogWarning("Please assign an item to add.");
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inventory Statistics", EditorStyles.boldLabel);

        // Total number of items
        if (inventoryManager.playerInventoryGrid == null)
            return;
        
        int totalItems = inventoryManager.playerInventoryGrid.GetAllItems().Count;
        foreach (var slot in inventoryManager.equipmentSlots)
        {
            if (slot.GetItem() != null)
                totalItems++;
        }

        EditorGUILayout.LabelField($"Total Items: {totalItems}");

        // Total number of itemViews
        int totalItemViews = inventoryManager.GetItemViewCount();
        EditorGUILayout.LabelField($"Total ItemViews: {totalItemViews}");
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Equipment Slots", EditorStyles.boldLabel);

        // Display equipment slot contents
        for (int i = 0; i < inventoryManager.equipmentSlots.Count; i++)
        {
            var slot = inventoryManager.equipmentSlots[i];
            var item = slot.GetItem();
            EditorGUILayout.LabelField($"Slot {i}: {(item != null ? item.ItemData.ItemName : "Empty")}");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Items", EditorStyles.boldLabel);

        // Display grid items
        var gridItems = inventoryManager.playerInventoryGrid.GetAllItems();
        foreach (var item in gridItems)
        {
            EditorGUILayout.LabelField($"Item: {item.ItemData.ItemName}, Position: ({item.GridX}, {item.GridY})");
        }
    }
}