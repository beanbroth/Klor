using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using static UnityEditor.Progress;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class S_InventoryGrid : MonoBehaviour, IItemInventory
{
    public int width = 7;
    public int height = 9;
    public GameObject cellPrefab;
    public float cellSize = 27f;

    [SerializeField] private Transform slotsParent;
    [SerializeField] private Transform itemsParent;

    [SerializeField] private Color baseSlotColor = Color.gray;
    [SerializeField] private Color validPlacementColor = new Color(0, 1, 0, 0.5f);
    [SerializeField] private Color invalidPlacementColor = new Color(1, 0, 0, 0.5f);

    private RectTransform gridRectTransform;
    private InventoryGridLogic gridLogic;

    public Transform ItemsParent => itemsParent;
    private List<Image> highlightedCells = new List<Image>();

    public RectTransform RectTransform { get; private set; }
    public float CellSize => cellSize;

    public void ManualAwake()
    {
        Debug.Log("awake called");
        RectTransform = GetComponent<RectTransform>();
        CreateParentObjects();
        gridLogic = new InventoryGridLogic(width, height, cellSize);
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
    }

    public bool CanPlaceItem(BaseItemInstance item, int x, int y, BaseItemInstance ignoredItem = null)
    {
        return gridLogic.CanPlaceItem(item, x, y, ignoredItem);
    }

    public bool PlaceItem(BaseItemInstance item, int x, int y)
    {
        if (gridLogic.PlaceItem(item, x, y))
        {
            InventoryManager.Instance.UpdateItemView(item);
            return true;
        }
        return false;
    }

    public bool RemoveItem(BaseItemInstance item)
    {
        return gridLogic.RemoveItem(item);
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

        if (gridLogic.CanPlaceItem(item, gridX, gridY, item))
        {
            gridLogic.RemoveItem(item);
            item.UpdatePosition(gridX, gridY);
            gridLogic.PlaceItem(item, gridX, gridY);
            InventoryManager.Instance.UpdateItemView(item);

            ClearHighlight();
            return true;
        }

        //return the item to its original position, with a rotation that fits. It's brute force, but works.

        gridLogic.RotateObjectToFit(item);

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
        if (gridLogic.AddItem(item))
        {
            return true;
        }
        return false;
    }

    public void HandleItemRotation(BaseItemInstance item)
    {
        gridLogic.HandleItemRotation(item);
        HighlightCells(item, item.GridX, item.GridY);
    }

    public List<BaseItemInstance> GetAllItems()
    {
        return gridLogic.GetItems();
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