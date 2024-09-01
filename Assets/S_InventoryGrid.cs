using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class S_InventoryGrid : MonoBehaviour, IInventorySlot
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
    [SerializeField] private Color baseSlotColor = Color.gray;
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
                    cellRect.GetComponent<Image>().color = baseSlotColor;
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

    public bool CanPlaceItem(S_InventoryItem item, int x, int y, S_InventoryItem ignoredItem = null)
    {
        if (x < 0 || y < 0 || x + item.CurrentWidth > width || y + item.CurrentHeight > height)
            return false;

        for (int i = 0; i < item.CurrentWidth; i++)
        {
            for (int j = 0; j < item.CurrentHeight; j++)
            {
                if (item.CurrentShape[i, j])
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

            if (item.GridX <= x && x < item.GridX + item.CurrentWidth &&
                item.GridY <= y && y < item.GridY + item.CurrentHeight)
            {
                if (item.CurrentShape[x - item.GridX, y - item.GridY])
                    return true;
            }
        }
        return false;
    }

    public void PlaceItem(S_InventoryItem item, int x, int y)
    {
        if (CanPlaceItem(item, x, y))
        {
            item.UpdatePosition(x, y);
            items.Add(item);
        }
    }

    public bool RemoveItem(S_InventoryItem item)
    {
        items.Remove(item);
        DestroyImmediate(item.gameObject);
        return true;
    }

    public bool HandleItemDrag(S_InventoryItem item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        // Adjust localPoint to represent the center of the slot
        localPoint.x -= cellSize / 2;
        localPoint.y += cellSize / 2;

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        HighlightCells(item, gridX, gridY);

        return true;
    }

    public bool HandleItemDrop(S_InventoryItem item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        // Adjust localPoint to represent the center of the slot
        localPoint.x -= cellSize / 2;
        localPoint.y += cellSize / 2;

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        if (CanPlaceItem(item, gridX, gridY, item))
        {
            item.transform.SetParent(itemsParent);
            // Remove the item from its original position
            items.Remove(item);

            // Update the item's position
            item.UpdatePosition(gridX, gridY);

            // Add the item back to the list at its new position
            items.Add(item);

            ClearHighlight();
            return true;
        }

        ClearHighlight();
        return false;
    }


    private void HighlightCells(S_InventoryItem item, int x, int y)
    {
        ClearHighlight();

        bool canPlace = CanPlaceItem(item, x, y, item);
        Color highlightColor = canPlace ? validPlacementColor : invalidPlacementColor;

        for (int i = 0; i < item.CurrentWidth; i++)
        {
            for (int j = 0; j < item.CurrentHeight; j++)
            {
                if (item.CurrentShape[i, j])
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
            cell.color = baseSlotColor;
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

        GameObject itemObject = new GameObject(itemToPlace.itemName);
        itemObject.transform.SetParent(itemsParent, false);

        S_InventoryItem newItem = itemObject.AddComponent<S_InventoryItem>();
        newItem.Initialize(itemToPlace, gridX, gridY, cellSize);

        if (CanPlaceItem(newItem, gridX, gridY))
        {
            PlaceItem(newItem, gridX, gridY);
        }
        else
        {
            DestroyImmediate(itemObject);
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

        // Create and place test items in corners
        PlaceTestItem(0, 0);
        PlaceTestItem(width - testItemData.Width, 0);
        PlaceTestItem(0, height - testItemData.Height);
        PlaceTestItem(width - testItemData.Width, height - testItemData.Height);
    }

    private void PlaceTestItem(int x, int y)
    {
        GameObject itemObject = new GameObject(testItemData.itemName);
        itemObject.transform.SetParent(itemsParent, false);

        S_InventoryItem newItem = itemObject.AddComponent<S_InventoryItem>();
        newItem.Initialize(testItemData, x, y, cellSize);

        if (CanPlaceItem(newItem, x, y))
        {
            PlaceItem(newItem, x, y);
        }
        else
        {
            DestroyImmediate(itemObject);
        }
    }

    // Add this method to handle item rotation
    public void HandleItemRotation(S_InventoryItem item)
    {
        if (CanPlaceItem(item, item.GridX, item.GridY, item))
        {
            // The rotated item can be placed in its current position
            HighlightCells(item, item.GridX, item.GridY);
        }
        else
        {
            // The rotated item cannot be placed in its current position
            // You might want to implement logic to find a new valid position or revert the rotation
            item.Rotate(); // Rotate back to the previous state
            HighlightCells(item, item.GridX, item.GridY);
        }
    }

    public bool CanAcceptItem(S_InventoryItem item)
    {
        throw new System.NotImplementedException();
    }

    public bool AddItem(S_InventoryItem item)
    {
        throw new System.NotImplementedException();
    }

    public Vector2 GetSlotPosition()
    {
        throw new System.NotImplementedException();
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
