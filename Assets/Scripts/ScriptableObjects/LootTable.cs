using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "LootTable", menuName = "Inventory/Loot Table")]
public class LootTable : ScriptableObject
{
    public List<BaseItemData> allItems = new List<BaseItemData>();
}

#if UNITY_EDITOR
[CustomEditor(typeof(LootTable))]
public class ItemDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        LootTable database = (LootTable)target;

        if (GUILayout.Button("Populate Items"))
        {
            PopulateItems(database);
        }
    }

    private void PopulateItems(LootTable database)
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseItemData");
        database.allItems = guids.Select(guid => AssetDatabase.LoadAssetAtPath<BaseItemData>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(item => item != null)
            .ToList();

        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
    }
}
#endif