using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemView : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public BaseItemInstance Item { get; private set; }

    private RectTransform rectTransform;
    private InventoryManager inventoryManager;
    private Vector2 originalPosition;
    private CanvasGroup canvasGroup;
    private float cellSize;
    private bool isDragging = false;
    private Vector2 dragPosition;

    private GameObject shapeOutline;
    private GameObject itemImageObject;
    private Image itemImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        shapeOutline = new GameObject("ShapeOutline");
        shapeOutline.transform.SetParent(transform, false);

        itemImageObject = new GameObject("ItemImage");
        itemImageObject.transform.SetParent(transform, false);
        itemImage = itemImageObject.AddComponent<Image>();
        itemImage.preserveAspect = true;
    }

    private void Start()
    {
        inventoryManager = InventoryManager.Instance;
    }

    public void Initialize(BaseItemInstance item, float cellSize)
    {
        Item = item;
        this.cellSize = cellSize;

        if (rectTransform == null)
        {
            //TODO: Make prefab?
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        UpdateSizeAndPosition();
        itemImage.sprite = Item.ItemData.ItemSprite;
        CreateShapeOutline();
        itemImageObject.transform.SetAsLastSibling();
        itemImageObject.transform.rotation = Quaternion.Euler(0, 0, -90 * (int)Item.CurrentRotation);
    }

    private void CreateShapeOutline()
    {
        foreach (Transform child in shapeOutline.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < Item.CurrentWidth; i++)
        {
            for (int j = 0; j < Item.CurrentHeight; j++)
            {
                if (Item.CurrentShape[i, j])
                {
                    GameObject cellOutline = new GameObject($"CellOutline_{i}_{j}");
                    cellOutline.transform.SetParent(shapeOutline.transform, false);
                    RectTransform cellRect = cellOutline.AddComponent<RectTransform>();
                    cellRect.pivot = new Vector2(0, 1);
                    cellRect.anchorMin = new Vector2(0, 1);
                    cellRect.anchorMax = new Vector2(0, 1);
                    cellRect.anchoredPosition = new Vector2(i * cellSize, -j * cellSize);
                    cellRect.sizeDelta = new Vector2(cellSize, cellSize);
                    Image outlineImage = cellOutline.AddComponent<Image>();
                    outlineImage.color = new Color(1, 1, 1, 0.5f); // Semi-transparent white
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            isDragging = true;
             
            transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            originalPosition = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            transform.SetAsLastSibling();

            MoveToMousePosition(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnEndDrag(eventData);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Handle right-click action (e.g., use item, show context menu)
            //inventoryManager.HandleItemRightClick(Item);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            MoveToMousePosition(eventData);
            inventoryManager.HandleItemDrag(Item, eventData.position);
        }
    }

    private void MoveToMousePosition(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            Vector2 centerOffset = new Vector2(Item.CurrentWidth * cellSize / 2, -Item.CurrentHeight * cellSize / 2);
            dragPosition = localPoint - centerOffset;
            rectTransform.anchoredPosition = dragPosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            isDragging = false;
            canvasGroup.alpha = 1f;
            
            
            canvasGroup.blocksRaycasts = true;

            if (inventoryManager.HandleItemDrop(Item, eventData.position))
            {
                // The item was successfully placed
                UpdateView();
            }
            else
            {
                ReturnToOriginalPosition();
            }
        }
    }

    private void Update()
    {
        if (isDragging && Input.GetMouseButtonDown(1))
        {
            Item.Rotate();
            UpdateView();
            inventoryManager.HandleItemDrag(Item, Input.mousePosition);
        }
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    public void UpdateView()
    {
        UpdateSizeAndPosition();
        CreateShapeOutline();
        itemImageObject.transform.rotation = Quaternion.Euler(0, 0, -90 * (int)Item.CurrentRotation);
    }

    private void UpdateSizeAndPosition()
    {
        rectTransform.sizeDelta = new Vector2(Item.CurrentWidth * cellSize, Item.CurrentHeight * cellSize);

        RectTransform itemImageRect = itemImageObject.GetComponent<RectTransform>();
        itemImageRect.sizeDelta = rectTransform.sizeDelta;
        itemImageRect.anchorMin = Vector2.zero;
        itemImageRect.anchorMax = Vector2.one;
        itemImageRect.offsetMin = Vector2.zero;
        itemImageRect.offsetMax = Vector2.zero;

        if (!isDragging)
        {
            rectTransform.anchoredPosition = new Vector2(Item.GridX * cellSize, -Item.GridY * cellSize);
        }
    }
}