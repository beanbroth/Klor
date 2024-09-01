using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class S_InventoryGrid : MonoBehaviour
{
    public int width = 7;
    public int height = 9;
    public GameObject cellPrefab;
    public float cellSize = 27f;
    private List<S_InventoryItem> items = new List<S_InventoryItem>();

    public SO_InventoryItemData testItemData;

    private RectTransform gridRectTransform;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform itemsParent;

    // Variables for highlighting
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);
    private List<Image> highlightedCells = new List<Image>();

    public RectTransform RectTransform => gridRectTransform;
    public float CellSize => cellSize;

    void Awake()
    {
        gridRectTransform = GetComponent<RectTransform>();
        if (gridRectTransform == null)
        {
            Debug.LogError("InventoryGrid must have a RectTransform component!");
        }

        CreateParentObjects();
    }

    void Start()
    {
        CreateVisualGrid();
    }

    private void CreateParentObjects()
    {
        if (slotsParent == null)
        {
            GameObject slotsParentObj = new GameObject("Slots");
            slotsParent = slotsParentObj.transform;
            slotsParent.SetParent(transform, false);
            slotsParent.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            slotsParent.GetComponent<RectTransform>().anchorMax = Vector2.one;
            slotsParent.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }

        if (itemsParent == null)
        {
            GameObject itemsParentObj = new GameObject("Items");
            itemsParent = itemsParentObj.transform;
            itemsParent.SetParent(transform, false);
            itemsParent.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            itemsParent.GetComponent<RectTransform>().anchorMax = Vector2.one;
            itemsParent.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        }
    }

    public void CreateVisualGrid()
    {
        if (gridRectTransform == null)
        {
            gridRectTransform = GetComponent<RectTransform>();
        }
        DestroyVisualGrid();

        gridRectTransform.sizeDelta = new Vector2(width * cellSize, height * cellSize);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                GameObject cell = Instantiate(cellPrefab, slotsParent);
                cell.name = $"Cell_X{x}_Y{y}";
                RectTransform cellRect = cell.GetComponent<RectTransform>();
                if (cellRect != null)
                {
                    cellRect.pivot = new Vector2(0, 1);
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
        for (int i = slotsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(slotsParent.GetChild(i).gameObject);
        }

        for (int i = itemsParent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(itemsParent.GetChild(i).gameObject);
        }

        items.Clear();
    }

    public bool CanPlaceItem(SO_InventoryItemData itemData, int x, int y, S_InventoryItem ignoredItem = null)
    {
        if (x < 0 || y < 0 || x + itemData.Width > width || y + itemData.Height > height)
            return false;

        for (int i = 0; i < itemData.Width; i++)
        {
            for (int j = 0; j < itemData.Height; j++)
            {
                if (itemData.Shape[i, j])
                {
                    if (IsOccupied(x + i, y + j, ignoredItem))
                        return false;
                }
            }
        }
        return true;
    }

    private bool IsOccupied(int x, int y, S_InventoryItem ignoredItem)
    {
        foreach (var item in items)
        {
            if (item == ignoredItem) continue; // Skip the item being dragged

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
            itemObject.transform.SetParent(itemsParent, false);

            RectTransform rectTransform = itemObject.AddComponent<RectTransform>();
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);

            S_InventoryItem newItem = itemObject.AddComponent<S_InventoryItem>();
            newItem.Initialize(itemData, x, y, cellSize);

            rectTransform.anchoredPosition = new Vector2(x * cellSize, -y * cellSize);
            rectTransform.sizeDelta = new Vector2(itemData.Width * cellSize, itemData.Height * cellSize);

            items.Add(newItem);
        }
    }

    public void RemoveItem(S_InventoryItem item)
    {
        items.Remove(item);
        DestroyImmediate(item.gameObject);
    }

    public void HandleItemDrag(S_InventoryItem item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        HighlightCells(item.ItemData, gridX, gridY, item);
    }

    public bool HandleItemDrop(S_InventoryItem item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        if (CanPlaceItem(item.ItemData, gridX, gridY, item))
        {
            // Remove the item from its original position
            items.Remove(item);

            // Update the item's position
            item.UpdatePosition(gridX, gridY, cellSize);

            // Add the item back to the list at its new position
            items.Add(item);

            ClearHighlight();
            return true;
        }

        ClearHighlight();
        return false;
    }

    private void HighlightCells(SO_InventoryItemData itemData, int x, int y, S_InventoryItem ignoredItem)
    {
        ClearHighlight();

        bool canPlace = CanPlaceItem(itemData, x, y, ignoredItem);
        Color highlightColor = canPlace ? validPlacementColor : invalidPlacementColor;

        for (int i = 0; i < itemData.Width; i++)
        {
            for (int j = 0; j < itemData.Height; j++)
            {
                if (itemData.Shape[i, j])
                {
                    int cellX = x + i;
                    int cellY = y + j;

                    if (cellX >= 0 && cellX < width && cellY >= 0 && cellY < height)
                    {
                        Image cellImage = slotsParent.GetChild(cellY * width + cellX).GetComponent<Image>();
                        cellImage.color = highlightColor;
                        highlightedCells.Add(cellImage);
                    }
                }
            }
        }
    }

    private void ClearHighlight()
    {
        foreach (var cell in highlightedCells)
        {
            cell.color = Color.white; // Or whatever the default color is
        }
        highlightedCells.Clear();
    }

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

    public void PlaceTestItems()
    {
        if (testItemData == null)
        {
            Debug.LogError("Test item data is not assigned!");
            return;
        }

        // Clear existing items
        for (int i = items.Count - 1; i >= 0; i--)
        {
            RemoveItem(items[i]);
        }

        // Top-left corner of the grid
        if (CanPlaceItem(testItemData, 0, 0))
            PlaceItem(testItemData, 0, 0);

        // Top-right corner of the grid
        if (CanPlaceItem(testItemData, width - testItemData.Width, 0))
            PlaceItem(testItemData, width - testItemData.Width, 0);

        // Bottom-left corner of the grid
        if (CanPlaceItem(testItemData, 0, height - testItemData.Height))
            PlaceItem(testItemData, 0, height - testItemData.Height);

        // Bottom-right corner of the grid
        if (CanPlaceItem(testItemData, width - testItemData.Width, height - testItemData.Height))
            PlaceItem(testItemData, width - testItemData.Width, height - testItemData.Height);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_InventoryGrid))]
public class InventoryGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        S_InventoryGrid grid = (S_InventoryGrid)target;
        EditorGUILayout.Space();

        if (GUILayout.Button("Create Grid"))
        {
            grid.CreateVisualGrid();
        }

        if (GUILayout.Button("Destroy Grid"))
        {
            grid.DestroyVisualGrid();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Place Test Items in Corners"))
        {
            grid.PlaceTestItems();
        }
    }
}
#endif