using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class InventoryGrid : MonoBehaviour
{
    public int width = 7;
    public int height = 9;
    public GameObject cellPrefab;
    public float cellSize = 27f;
    private List<S_InventoryItem> items = new List<S_InventoryItem>();

    public SO_InventoryItemData testItemData;

    private RectTransform gridRectTransform;

    void Awake()
    {
        gridRectTransform = GetComponent<RectTransform>();
        if (gridRectTransform == null)
        {
            Debug.LogError("InventoryGrid must have a RectTransform component!");
        }
    }

    void Start()
    {
        CreateVisualGrid();
        PlaceTestItems();
    }

    public void CreateVisualGrid()
    {
        // Clear existing cells
        DestroyVisualGrid();

        // Set the size of the grid
        gridRectTransform.sizeDelta = new Vector2(width * cellSize, height * cellSize);

        // Create new cells
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject cell = Instantiate(cellPrefab, transform);
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                if (cellRect != null)
                {
                    cellRect.anchorMin = new Vector2(0, 1);
                    cellRect.anchorMax = new Vector2(0, 1);
                    cellRect.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                }
            }
        }
    }

    public void DestroyVisualGrid()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    public bool CanPlaceItem(SO_InventoryItemData itemData, int x, int y)
    {
        if (x < 0 || y < 0 || x + itemData.Width > width || y + itemData.Height > height)
            return false;

        for (int i = 0; i < itemData.Width; i++)
        {
            for (int j = 0; j < itemData.Height; j++)
            {
                if (itemData.Shape[i, j])
                {
                    if (IsOccupied(x + i, y + j))
                        return false;
                }
            }
        }
        return true;
    }

    private bool IsOccupied(int x, int y)
    {
        foreach (var item in items)
        {
            if (item.GridX <= x && x < item.GridX + item.ItemData.Width &&
                item.GridY <= y && y < item.GridY + item.ItemData.Height)
            {
                if (item.ItemData.Shape[x - item.GridX, y - item.GridY])
                    return true;
            }
        }
        return false;
    }

    public void PlaceItem(SO_InventoryItemData itemData, int x, int y)
    {
        if (CanPlaceItem(itemData, x, y))
        {
            GameObject itemObject = new GameObject(itemData.itemName);
            itemObject.transform.SetParent(transform, false);

            RectTransform rectTransform = itemObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);

            S_InventoryItem newItem = itemObject.AddComponent<S_InventoryItem>();
            newItem.Initialize(itemData, x, y, cellSize);

            // Force the item to the correct position
            rectTransform.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
            rectTransform.sizeDelta = new Vector2(itemData.Width * cellSize, itemData.Height * cellSize);

            items.Add(newItem);
        }
    }

    public void RemoveItem(S_InventoryItem item)
    {
        items.Remove(item);
        Destroy(item.gameObject);
    }

    // Method to handle item placement based on mouse input
    public void HandleItemPlacement(SO_InventoryItemData itemToPlace)
    {
        Vector2 mousePosition = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            GetComponent<RectTransform>(),
            mousePosition,
            null,
            out Vector2 localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        if (CanPlaceItem(itemToPlace, gridX, gridY))
        {
            PlaceItem(itemToPlace, gridX, gridY);
        }
    }

    private void PlaceTestItems()
    {
        if (testItemData == null)
        {
            Debug.LogError("Test item data is not assigned!");
            return;
        }

        // Top-left corner of the grid
        PlaceItem(testItemData, 0, 0);

        // Top-right corner of the grid
        PlaceItem(testItemData, width - 1, 0);

        // Bottom-left corner of the grid
        PlaceItem(testItemData, 0, height - 1);

        // Bottom-right corner of the grid
        PlaceItem(testItemData, width - 1, height - 1);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(InventoryGrid))]
public class InventoryGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        InventoryGrid grid = (InventoryGrid)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Grid"))
        {
            grid.CreateVisualGrid();
        }
        if (GUILayout.Button("Destroy Grid"))
        {
            grid.DestroyVisualGrid();
        }
    }
}
#endif