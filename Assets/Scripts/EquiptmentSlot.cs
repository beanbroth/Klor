using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class EquipmentSlot : MonoBehaviour, IItemInventory
{
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image slotImage;
    
    [SerializeField] private EquipmentSlotType slotType;
    
    public event Action<BaseItemInstance> OnItemChanged;

    public RectTransform RectTransform => rectTransform;

    public BaseItemInstance CurrentItem => currentItem;

    private BaseItemInstance currentItem;

    public void ManualAwake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (slotImage == null) slotImage = GetComponent<Image>();
    }

    public bool CanAcceptItem(BaseItemInstance item)
    {
        return currentItem == null && item.ItemData.EquipmentSlotType == slotType;
    }

    public bool AddItem(BaseItemInstance item)
    {
        if (CanAcceptItem(item))
        {
            RemoveCurrentItem();
            currentItem = item;
            currentItem.UpdatePosition(0, 0);
            OnItemChanged?.Invoke(currentItem);
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
            OnItemChanged?.Invoke(null);
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