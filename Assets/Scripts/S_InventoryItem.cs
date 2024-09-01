using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public enum RotationState
{
    Rotation0,
    Rotation90,
    Rotation180,
    Rotation270
}

public class S_InventoryItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler, IPointerUpHandler
{
    public SO_InventoryItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public RotationState CurrentRotation { get; private set; }
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public bool[,] CurrentShape { get; private set; }

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

        // Create a new GameObject for the shape outline
        shapeOutline = new GameObject("ShapeOutline");
        shapeOutline.transform.SetParent(transform, false);

        // Create a new GameObject for the item image
        itemImageObject = new GameObject("ItemImage");
        itemImageObject.transform.SetParent(transform, false);
        itemImage = itemImageObject.AddComponent<Image>();
        itemImage.preserveAspect = true;
    }

    private void Start()
    {
        inventoryManager = InventoryManager.Instance;

    }

    public void Initialize(SO_InventoryItemData itemData, int x, int y, float cellSize)
    {
        ItemData = itemData;
        GridX = x;
        GridY = y;
        this.cellSize = cellSize;
        CurrentRotation = RotationState.Rotation0;
        CurrentWidth = itemData.Width;
        CurrentHeight = itemData.Height;
        CurrentShape = (bool[,])itemData.Shape.Clone();

        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);

        UpdateSizeAndPosition();

        // Set the sprite for the item image
        itemImage.sprite = ItemData.itemSprite;

        CreateShapeOutline();

        // Ensure the item image is on top
        itemImageObject.transform.SetAsLastSibling();
    }

    private void CreateShapeOutline()
    {
        // Clear existing child objects in the shape outline
        foreach (Transform child in shapeOutline.transform)
        {
            Destroy(child.gameObject);
        }

        // Create child objects for each filled cell in the item's shape
        for (int i = 0; i < CurrentWidth; i++)
        {
            for (int j = 0; j < CurrentHeight; j++)
            {
                if (CurrentShape[i, j])
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
            originalPosition = rectTransform.anchoredPosition;
            canvasGroup.alpha = 0.6f;
            canvasGroup.blocksRaycasts = false;
            transform.SetAsLastSibling();

            MoveToMousePosition(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
       //if the right mouse is clicked, do nothing (for now, eventually EAT)
       //if the left mouse is clicked, place the item where it is
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnEndDrag(eventData);
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            MoveToMousePosition(eventData);
            inventoryManager.HandleItemDrag(this, eventData.position);
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
            Vector2 centerOffset = new Vector2(CurrentWidth * cellSize / 2, -CurrentHeight * cellSize / 2);
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

            if (inventoryManager.HandleItemDrop(this, eventData.position))
            {
                // The item was successfully placed
            }
            else
            {
                ReturnToOriginalPosition();
                inventoryManager.PlaceItem(this, GridX, GridY);
            }
        }
    }

    private void Update()
    {
        if (isDragging && Input.GetMouseButtonDown(1))
        {
            Rotate();
        }
    }

    public void ReturnToOriginalPosition()
    {
        rectTransform.anchoredPosition = originalPosition;
    }

    public void UpdatePosition(int newX, int newY)
    {
        GridX = newX;
        GridY = newY;
        rectTransform.anchoredPosition = new Vector2(newX * cellSize, -newY * cellSize);
    }

    public void Rotate()
    {
        CurrentRotation = (RotationState)(((int)CurrentRotation + 1) % 4);
        UpdateShapeAndSize();
        UpdateSizeAndPosition();
        CreateShapeOutline();

        itemImageObject.transform.rotation = Quaternion.Euler(0, 0, -90 * (int)CurrentRotation);

        if (isDragging)
        {
            rectTransform.anchoredPosition = dragPosition;

            if (inventoryManager != null)
            {
                inventoryManager.HandleItemDrag(this, Input.mousePosition);
            }
        }
    }

    private void UpdateShapeAndSize()
    {
        switch (CurrentRotation)
        {
            case RotationState.Rotation0:
            case RotationState.Rotation180:
                CurrentWidth = ItemData.Width;
                CurrentHeight = ItemData.Height;
                break;
            case RotationState.Rotation90:
            case RotationState.Rotation270:
                CurrentWidth = ItemData.Height;
                CurrentHeight = ItemData.Width;
                break;
        }

        CurrentShape = new bool[CurrentWidth, CurrentHeight];
        for (int i = 0; i < ItemData.Width; i++)
        {
            for (int j = 0; j < ItemData.Height; j++)
            {
                switch (CurrentRotation)
                {
                    case RotationState.Rotation0:
                        CurrentShape[i, j] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation90:
                        CurrentShape[ItemData.Height - 1 - j, i] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation180:
                        CurrentShape[ItemData.Width - 1 - i, ItemData.Height - 1 - j] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation270:
                        CurrentShape[j, ItemData.Width - 1 - i] = ItemData.Shape[i, j];
                        break;
                }
            }
        }
    }



    private void UpdateSizeAndPosition()
    {
        rectTransform.sizeDelta = new Vector2(CurrentWidth * cellSize, CurrentHeight * cellSize);

        // Update the item image size and position
        RectTransform itemImageRect = itemImageObject.GetComponent<RectTransform>();
        itemImageRect.sizeDelta = rectTransform.sizeDelta;
        itemImageRect.anchorMin = Vector2.zero;
        itemImageRect.anchorMax = Vector2.one;
        itemImageRect.offsetMin = Vector2.zero;
        itemImageRect.offsetMax = Vector2.zero;

        if (!isDragging)
        {
            UpdatePosition(GridX, GridY);
        }
    }
}