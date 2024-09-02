using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class EquipmentSlot : MonoBehaviour, IItemInventory
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image slotImage;

    public RectTransform RectTransform => rectTransform;

    private BaseItemInstance currentItem;

    public void ManualAwake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (slotImage == null) slotImage = GetComponent<Image>();
    }

    public bool CanAcceptItem(BaseItemInstance item)
    {
        // TODO: Implement proper checking based on item type and slot type. For now just make sure the slot is empty.
        return currentItem == null;
    }

    public bool AddItem(BaseItemInstance item)
    {
        if (CanAcceptItem(item))
        {
            RemoveCurrentItem();
            currentItem = item;
            currentItem.UpdatePosition(0, 0);
            return true;
        }
        return false;
    }

    public bool RemoveItem(BaseItemInstance item)
    {
        if (currentItem == item)
        {
            RemoveCurrentItem();
            return true;
        }
        return false;
    }

    private void RemoveCurrentItem()
    {
        if (currentItem != null)
        {
            var oldItem = currentItem;
            currentItem = null;
        }
    }

    public Vector2 GetSlotPosition()
    {
        return rectTransform.position;
    }

    public void ClearItem()
    {
        RemoveCurrentItem();
    }

    public BaseItemInstance GetItem()
    {
        return currentItem;
    }

    public void HighlightSlot(bool canAccept)
    {
        Color highlightColor = canAccept ? Color.green : Color.red;
        slotImage.color = Color.Lerp(Color.white, highlightColor, 0.5f);
    }

    public void ResetHighlight()
    {
        slotImage.color = Color.white;
    }
}