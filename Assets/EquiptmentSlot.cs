using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlot : MonoBehaviour, IInventorySlot
{
    public string SlotType; // e.g., "Helmet", "Weapon", "Armor"
    private S_InventoryItem currentItem;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public bool CanAcceptItem(S_InventoryItem item)
    {
        // Check if the slot is empty and if the item type matches the slot type
        //currentItem == null && item.ItemData.ItemType.ToString() == SlotType;
        return true;
    }

    public bool AddItem(S_InventoryItem item)
    {
        if (CanAcceptItem(item))
        {
            Debug.Log("item added");
            currentItem = item;
            item.transform.SetParent(transform);
            item.transform.localPosition = Vector3.zero;
            return true;
        }
        return false;
    }

    public bool RemoveItem(S_InventoryItem item)
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
}