using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class S_InventoryItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public SO_InventoryItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    private RectTransform rectTransform;
    private Canvas canvas;
    private S_InventoryGrid inventoryGrid;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;
    private Vector2 dragOffset;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
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

        // Calculate the offset between the cursor and the item's position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint);
        dragOffset = rectTransform.anchoredPosition - localPoint;

        // Immediately start dragging
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pointerPosition = eventData.position;
        Vector2 localPointerPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, pointerPosition, eventData.pressEventCamera, out localPointerPosition))
        {
            rectTransform.anchoredPosition = localPointerPosition + dragOffset;
        }

        inventoryGrid.HandleItemDrag(this, eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        if (!inventoryGrid.HandleItemDrop(this, eventData.position))
        {
            ReturnToOriginalPosition();
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