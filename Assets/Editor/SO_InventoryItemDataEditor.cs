using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseItemData), true)]
public class SO_InventoryItemDataEditor : Editor
{
    private BaseItemData item;
    private bool[,] editableShape;

    private void OnEnable()
    {
        item = (BaseItemData)target;
        EnsureMinimumSize();
        UpdateEditableShape();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Item Shape Helper", EditorStyles.boldLabel);

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
                EditorGUI.BeginChangeCheck();
                editableShape[x, y] = EditorGUILayout.Toggle(editableShape[x, y], GUILayout.Width(20));
                if (EditorGUI.EndChangeCheck())
                {
                    // Apply changes immediately when a checkbox is clicked
                    ApplyChanges();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EnsureMinimumSize();
            UpdateEditableShape();
            ApplyChanges();
        }
    }

    private void EnsureMinimumSize()
    {
        bool sizeChanged = false;
        if (item.width < 1)
        {
            item.width = 1;
            sizeChanged = true;
        }
        if (item.height < 1)
        {
            item.height = 1;
            sizeChanged = true;
        }

        // Ensure shapeData has exactly width * height elements
        int requiredSize = item.width * item.height;
        if (item.shapeData.Count != requiredSize)
        {
            if (item.shapeData.Count < requiredSize)
            {
                while (item.shapeData.Count < requiredSize)
                {
                    item.shapeData.Add(false);
                }
            }
            else
            {
                item.shapeData.RemoveRange(requiredSize, item.shapeData.Count - requiredSize);
            }
            sizeChanged = true;
        }

        if (sizeChanged)
        {
            EditorUtility.SetDirty(item);
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
                editableShape[x, y] = item.shapeData[index];
            }
        }
    }

    private void ApplyChanges()
    {
        Undo.RecordObject(item, "Modify Item Shape");
        for (int y = 0; y < item.height; y++)
        {
            for (int x = 0; x < item.width; x++)
            {
                int index = y * item.width + x;
                item.shapeData[index] = editableShape[x, y];
            }
        }
        EditorUtility.SetDirty(item);
    }
}