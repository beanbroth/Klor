using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IItemInventory
{
    private BaseItemInstance currentItem;
    private RectTransform rectTransform;
    private Image slotImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        slotImage = GetComponent<Image>();
        if (slotImage == null)
        {
            slotImage = gameObject.AddComponent<Image>();
        }
    }

    public bool CanAcceptItem(BaseItemInstance item)
    {
        // TODO: Implement proper checking based on item type and slot type
        return currentItem == null;
    }

    public bool AddItem(BaseItemInstance item)
    {
        if (CanAcceptItem(item))
        {
            currentItem = item;
            //currentItem.CurrentRotation = RotationState.Rotation0;
            currentItem.UpdatePosition(0, 0);
            InventoryManager.Instance.UpdateItemView(item);
            return true;
        }
        return false;
    }

    public bool RemoveItem(BaseItemInstance item)
    {
        if (currentItem == item)
        {
            currentItem = null;
            return true;
        }
        return false;
    }

    public Vector2 GetSlotPosition()
    {
        return rectTransform.position;
    }

    public void ClearItem()
    {
        currentItem = null;
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