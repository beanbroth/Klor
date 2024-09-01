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
                    Debug.Log($"Item '{itemToAdd.itemName}' added successfully.");
                }
                else
                {
                    Debug.LogWarning($"Failed to add item '{itemToAdd.itemName}'. Inventory might be full.");
                }
            }
            else
            {
                Debug.LogWarning("Please assign an item to add.");
            }
        }
    }
}