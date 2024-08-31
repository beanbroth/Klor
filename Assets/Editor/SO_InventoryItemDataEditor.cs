using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SO_InventoryItemData))]
public class SO_InventoryItemDataEditor : Editor
{
    private SO_InventoryItemData item;
    private bool[,] editableShape;

    private void OnEnable()
    {
        item = (SO_InventoryItemData)target;
        EnsureMinimumSize();
        UpdateEditableShape();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Shape", EditorStyles.boldLabel);

        // Ensure editableShape is up-to-date
        if (editableShape == null || editableShape.GetLength(0) != item.width || editableShape.GetLength(1) != item.height)
        {
            UpdateEditableShape();
        }

        // Draw the grid
        EditorGUILayout.BeginVertical();
        for (int y = 0; y < item.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < item.width; x++)
            {
                editableShape[x, y] = EditorGUILayout.Toggle(editableShape[x, y], GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        // Apply changes button
        if (GUILayout.Button("Apply Changes"))
        {
            ApplyChanges();
        }

        if (EditorGUI.EndChangeCheck())
        {
            EnsureMinimumSize();
            UpdateEditableShape();
        }
    }

    private void EnsureMinimumSize()
    {
        item.width = Mathf.Max(1, item.width);
        item.height = Mathf.Max(1, item.height);

        // Ensure shapeData has at least width * height elements
        while (item.shapeData.Count < item.width * item.height)
        {
            item.shapeData.Add(false);
        }
    }

    private void UpdateEditableShape()
    {
        editableShape = new bool[item.width, item.height];
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                int index = y * item.width + x;
                editableShape[x, y] = index < item.shapeData.Count ? item.shapeData[index] : false;
            }
        }
    }

    private void ApplyChanges()
    {
        Undo.RecordObject(item, "Modify Item Shape");
        item.shapeData.Clear();
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                item.shapeData.Add(editableShape[x, y]);
            }
        }
        EditorUtility.SetDirty(item);
    }
}