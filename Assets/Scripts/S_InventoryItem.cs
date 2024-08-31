using UnityEngine;
using UnityEngine.UI;

public class S_InventoryItem : MonoBehaviour
{
    public SO_InventoryItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public void Initialize(SO_InventoryItemData itemData, int x, int y, float cellSize)
    {
        ItemData = itemData;
        GridX = x;
        GridY = y;

        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.sizeDelta = new Vector2(itemData.Width * cellSize, itemData.Height * cellSize);

        // Create a child object for each filled cell in the item's shape
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
}