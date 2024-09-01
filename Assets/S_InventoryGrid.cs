using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class S_InventoryGrid : MonoBehaviour, IItemInventory
{
    public int width = 7;
    public int height = 9;
    public GameObject cellPrefab;
    public float cellSize = 27f;
    private List<BaseItemInstance> items = new List<BaseItemInstance>();
    private Dictionary<BaseItemInstance, ItemView> itemViews = new Dictionary<BaseItemInstance, ItemView>();

    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform itemsParent;

    [SerializeField] private Color baseSlotColor = Color.gray;
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);

    private RectTransform gridRectTransform;

    public Transform ItemsParent => itemsParent;
    private List<Image> highlightedCells = new List<Image>();

    public RectTransform RectTransform { get; private set; }
    public float CellSize => cellSize;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
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

    public bool CanPlaceItem(BaseItemInstance item, int x, int y, BaseItemInstance ignoredItem = null)
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

    private bool IsOccupied(int x, int y, BaseItemInstance ignoredItem)
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


    public bool PlaceItem(BaseItemInstance item, int x, int y)
    {
        if (CanPlaceItem(item, x, y))
        {
            item.UpdatePosition(x, y);
            items.Add(item);
            return true;
        }
        return false;
    }

    public bool RemoveItem(BaseItemInstance item)
    {
        return items.Remove(item);
    }

    public bool HandleItemDrag(BaseItemInstance item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            RectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        HighlightCells(item, gridX, gridY);

        return true;
    }

    public bool HandleItemDrop(BaseItemInstance item, Vector2 position)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            RectTransform,
            position,
            null,
            out Vector2 localPoint
        );

        int gridX = Mathf.FloorToInt((localPoint.x + (width * cellSize / 2)) / cellSize);
        int gridY = Mathf.FloorToInt(((height * cellSize / 2) - localPoint.y) / cellSize);

        if (CanPlaceItem(item, gridX, gridY, item))
        {
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
    private void HighlightCells(BaseItemInstance item, int x, int y)
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


    public bool AddItem(BaseItemInstance item)
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (CanPlaceItem(item, x, y))
                {
                    PlaceItem(item, x, y);
                    return true;
                }

                for (int r = 0; r < 3; r++)
                {
                    item.Rotate();
                    if (CanPlaceItem(item, x, y))
                    {
                        PlaceItem(item, x, y);
                        return true;
                    }
                }

                item.Rotate();
            }
        }
        return false;
    }

    public void HandleItemRotation(BaseItemInstance item)
    {
        if (CanPlaceItem(item, item.GridX, item.GridY, item))
        {
            if (itemViews.TryGetValue(item, out ItemView itemView))
            {
                itemView.UpdateView();
            }
            HighlightCells(item, item.GridX, item.GridY);
        }
        else
        {
            item.Rotate();
            if (itemViews.TryGetValue(item, out ItemView itemView))
            {
                itemView.UpdateView();
            }
            HighlightCells(item, item.GridX, item.GridY);
        }
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
    }
}
#endif