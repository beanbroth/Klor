using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<InventoryManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(InventoryManager).Name;
                    _instance = obj.AddComponent<InventoryManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private S_InventoryGrid playerInventoryGrid;
    [SerializeField] private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
    [SerializeField] private BaseItemData testItemData; // Add this for test items

    private Dictionary<BaseItemInstance, ItemView> itemViews = new Dictionary<BaseItemInstance, ItemView>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        equipmentSlots.AddRange(FindObjectsOfType<EquipmentSlot>());
    }

    public bool HandleItemDrag(BaseItemInstance item, Vector2 position)
    {
        foreach (var slot in equipmentSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.GetComponent<RectTransform>(), position))
            {
                if (slot.CanAcceptItem(item))
                {
                    // Implement highlight logic
                }
                return true;
            }
        }
        return playerInventoryGrid.HandleItemDrag(item, position);
    }

    public bool HandleItemDrop(BaseItemInstance item, Vector2 position)
    {
        // Store the original parent before attempting to move the item
        Transform originalParent = itemViews[item].transform.parent;

        // Try to add the item to an equipment slot
        foreach (var slot in equipmentSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.GetComponent<RectTransform>(), position))
            {
                if (slot.CanAcceptItem(item) && slot.AddItem(item))
                {
                    RemoveItemFromPreviousLocation(item);
                    UpdateItemViewParent(item, slot.transform);
                    return true;
                }
                return false;
            }
        }

        // Try to add the item to the grid inventory
        if (playerInventoryGrid.HandleItemDrop(item, position))
        {
            RemoveItemFromPreviousLocation(item);
            UpdateItemViewParent(item, playerInventoryGrid.ItemsParent);
            return true;
        }

        // If we reach here, the drop failed, so we keep the item in its orizginal location
        UpdateItemViewParent(item, originalParent);
        return false;
    }

    private void RemoveItemFromPreviousLocation(BaseItemInstance item)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot.RemoveItem(item))
            {
                return;
            }
        }
        playerInventoryGrid.RemoveItem(item);
    }

    private void UpdateItemViewParent(BaseItemInstance item, Transform newParent)
    {
        if (itemViews.TryGetValue(item, out ItemView itemView))
        {
            itemView.transform.SetParent(newParent, false);
            itemView.UpdateView(); // Update the view to reflect any changes
        }
        else
        {
            CreateItemView(item, newParent);
        }
    }


    public void HandleItemRightClick(BaseItemInstance item)
    {
        // Implement right-click functionality here
        Debug.Log($"Right-clicked on item: {item.ItemData.itemName}");
    }
    public bool CreateAndAddItem(BaseItemData itemData)
    {
        if (itemData == null)
        {
            Debug.LogError("Item data is not assigned!");
            return false;
        }

        BaseItemInstance newItem = new BaseItemInstance(itemData, 0, 0);
        return AddItem(newItem);
    }

    private void ClearAllItems()
    {
        // Clear items from inventory grid
        playerInventoryGrid.DestroyVisualGrid();
        playerInventoryGrid.CreateVisualGrid();

        // Clear items from equipment slots
        foreach (var slot in equipmentSlots)
        {
            slot.ClearItem();
        }
    }

    public ItemView CreateItemView(BaseItemInstance item, Transform parent)
    {
        if (itemViews.TryGetValue(item, out ItemView existingView))
        {
            return existingView;
        }

        GameObject itemViewObject = new GameObject(item.ItemData.itemName + "View");
        itemViewObject.transform.SetParent(parent, false);
        ItemView itemView = itemViewObject.AddComponent<ItemView>();
        itemView.Initialize(item, playerInventoryGrid.CellSize);
        itemViews[item] = itemView;

        return itemView;
    }

    public void UpdateItemView(BaseItemInstance item)
    {
        if (itemViews.TryGetValue(item, out ItemView itemView))
        {
            itemView.UpdateView();
        }
    }

    public void RemoveItemView(BaseItemInstance item)
    {
        if (itemViews.TryGetValue(item, out ItemView itemView))
        {
            Destroy(itemView.gameObject);
            itemViews.Remove(item);
        }
    }

    public void PlaceItem(BaseItemInstance item, int x, int y)
    {
        if (playerInventoryGrid.PlaceItem(item, x, y))
        {
            CreateItemView(item, playerInventoryGrid.ItemsParent);
            UpdateItemView(item);
        }
    }

    public bool AddItem(BaseItemInstance item)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot.CanAcceptItem(item) && slot.AddItem(item))
            {
                CreateItemView(item, slot.transform);
                return true;
            }
        }

        if (playerInventoryGrid.AddItem(item))
        {
            CreateItemView(item, playerInventoryGrid.ItemsParent);
            return true;
        }

        return false;
    }
}

public interface IItemInventory
{
    bool AddItem(BaseItemInstance item);

}