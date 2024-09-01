using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class S_InventoryItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public SO_InventoryItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    private RectTransform rectTransform;
    private S_InventoryGrid inventoryGrid;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;
    private Vector2 offset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        inventoryGrid = GetComponentInParent<S_InventoryGrid>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void Initialize(SO_InventoryItemData itemData, int x, int y, float cellSize)
    {
        ItemData = itemData;
        GridX = x;
        GridY = y;
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.sizeDelta = new Vector2(itemData.Width * cellSize, itemData.Height * cellSize);
        UpdatePosition(x, y, cellSize);

        // Create child objects for each filled cell in the item's shape
        for (int i = 0; i < itemData.Width; i++)
        {
            for (int j = 0; j < itemData.Height; j++)
            {
                if (itemData.Shape[i, j])
                {
                    GameObject cellObject = new GameObject($"Cell_{i}_{j}");
                    cellObject.transform.SetParent(transform, false);
                    RectTransform cellRect = cellObject.AddComponent<RectTransform>();
                    cellRect.pivot = new Vector2(0, 1);
                    cellRect.anchorMin = new Vector2(0, 1);
                    cellRect.anchorMax = new Vector2(0, 1);
                    cellRect.anchoredPosition = new Vector2(i * cellSize, -j * cellSize);
                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                    Image image = cellObject.AddComponent<Image>();
                    image.sprite = itemData.itemSprite;
                    image.preserveAspect = true;
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();

        // Calculate the offset between the mouse position and the item's position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);
        offset = rectTransform.anchoredPosition - localPoint;

        MoveToMousePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveToMousePosition(eventData);
        inventoryGrid.HandleItemDrag(this, eventData.position);
    }

    private void MoveToMousePosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint + offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        if (inventoryGrid.HandleItemDrop(this, eventData.position))
        {
            // The item was successfully placed
        }
        else
        {
            // If it can't be placed, return to original position
            ReturnToOriginalPosition();
            // Add the item back to its original position in the grid
            inventoryGrid.PlaceItem(ItemData, GridX, GridY);
        }
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    public void UpdatePosition(int newX, int newY, float cellSize)
    {
        GridX = newX;
        GridY = newY;
        rectTransform.anchoredPosition = new Vector2(newX * cellSize, -newY * cellSize);
    }
}