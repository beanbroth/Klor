using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;
    public static InventoryManager Instance
    {
        get
        {
            return _instance;
        }
    }

    [SerializeField] public S_InventoryGrid playerInventoryGrid;
    [SerializeField] public List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
    [SerializeField] public EquipmentSlot primaryWeaponSlot;
    [SerializeField] public EquipmentSlot secondaryWeaponSlot;
    
    [SerializeField] private BaseItemData testItemData;

    private Dictionary<BaseItemInstance, ItemView> itemViews = new Dictionary<BaseItemInstance, ItemView>();

    public void ManualAwake()
    {
        _instance = this;

        foreach (var slot in equipmentSlots)
        {
            slot.ManualAwake();
        }
        playerInventoryGrid.ManualAwake();
    }

    private void OnEnable()
    {
        if (!_instance)
        {
            ManualAwake();
        }
        RefreshAllItemViews();
    }
    
    
    private void RefreshAllItemViews()
    {
        // Clear existing item views
        foreach (var itemView in itemViews.Values)
        {
            Destroy(itemView.gameObject);
        }
        itemViews.Clear();

        // Recreate views for items in the grid
        foreach (var item in playerInventoryGrid.GetAllItems())
        {
            CreateItemView(item, playerInventoryGrid.ItemsParent);
        }

        // Recreate views for items in equipment slots
        foreach (var slot in equipmentSlots)
        {
            var item = slot.GetItem();
            if (item != null)
            {
                CreateItemView(item, slot.transform);
            }
        }
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
        Debug.Log($"HandleItemDrop started for item: {item.Name} at position: {position}");

        // Store the original parent before attempting to move the item
        Transform originalParent = itemViews[item].transform.parent;
        Debug.Log($"Original parent: {originalParent.name}");

        // Try to add the item to an equipment slot
        foreach (var slot in equipmentSlots)
        {
            Debug.Log($"Checking equipment slot: {slot.name}");
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.RectTransform, position))
            {
                Debug.Log($"Item is over slot: {slot.name}");
                if (slot.CanAcceptItem(item))
                {
                    Debug.Log($"Slot {slot.name} can accept item");
                    if (slot.AddItem(item))
                    {
                        Debug.Log($"Item added to slot {slot.name}");
                        RemoveItemFromPreviousLocation(item, slot.transform);
                        UpdateItemViewParent(item, slot.transform);
                        UpdateItemView(item);
                        Debug.Log("HandleItemDrop completed successfully (equipment slot)");
                        return true;
                    }
                    else
                    {
                        Debug.Log($"Failed to add item to slot {slot.name}");
                    }
                }
                else
                {
                    Debug.Log($"Slot {slot.name} cannot accept item");
                }
                Debug.Log("HandleItemDrop failed (invalid equipment slot)");
                return false;
            }
        }

        // Try to add the item to the grid inventory
        Debug.Log("Attempting to add item to grid inventory");
        if (playerInventoryGrid.HandleItemDrop(item, position))
        {
            Debug.Log("Item added to grid inventory");
            RemoveItemFromPreviousLocation(item, playerInventoryGrid.ItemsParent);
            UpdateItemViewParent(item, playerInventoryGrid.ItemsParent);
            Debug.Log("HandleItemDrop completed successfully (grid inventory)");
            return true;
        }

        // If we reach here, the drop failed, so we keep the item in its original location
        Debug.Log("Drop failed, keeping item in original location");
        UpdateItemViewParent(item, originalParent);
        Debug.Log("HandleItemDrop failed (no valid drop location)");
        return false;
    }

    private void RemoveItemFromPreviousLocation(BaseItemInstance item, Transform newParent)
    {
        Debug.Log($"RemoveItemFromPreviousLocation started for item: {item.Name}, new parent: {newParent.name}");

        Transform currentParent = itemViews[item].transform.parent;
        Debug.Log($"Current parent: {currentParent.name}");

        // Check if the item is moving to a different parent
        if (currentParent != newParent)
        {
            Debug.Log("Item is moving to a different parent");

            // First, try to remove from equipment slots
            bool removedFromEquipment = false;
            foreach (var slot in equipmentSlots)
            {
                if (slot.transform == currentParent)
                {
                    Debug.Log($"Removing item from equipment slot: {slot.name}");
                    if (slot.RemoveItem(item))
                    {
                        Debug.Log($"Item removed from equipment slot: {slot.name}");
                        removedFromEquipment = true;
                        break;
                    }
                }
            }

            // If not removed from equipment, then it must be in the grid
            if (!removedFromEquipment)
            {
                Debug.Log("Removing item from grid inventory");
                playerInventoryGrid.RemoveItem(item);
            }
        }
        else
        {
            Debug.Log("Item is staying in the same parent, no need to remove");
        }

        Debug.Log("RemoveItemFromPreviousLocation completed");
    }

    public void UpdateItemViewParent(BaseItemInstance item, Transform newParent)
    {
        if (itemViews.TryGetValue(item, out ItemView itemView))
        {
            itemView.transform.SetParent(newParent, false);
            UpdateItemView(item);
        }
        else if (gameObject.activeInHierarchy)
        {
            CreateItemView(item, newParent);
            UpdateItemView(item);
        }
    }

    public void HandleItemRightClick(BaseItemInstance item)
    {
        // Implement right-click functionality here
        Debug.Log($"Right-clicked on item: {item.ItemData.ItemName}");
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
        playerInventoryGrid.DestroyVisualGrid();
        playerInventoryGrid.CreateVisualGrid();

        foreach (var slot in equipmentSlots)
        {
            slot.ClearItem();
        }

        foreach (var itemView in itemViews.Values)
        {
            Destroy(itemView.gameObject);
        }
        itemViews.Clear();
    }

    private ItemView CreateItemView(BaseItemInstance item, Transform parent)
    {
        if (itemViews.TryGetValue(item, out ItemView existingView))
        {
            existingView.transform.SetParent(parent, false);
            return existingView;
        }

        GameObject itemViewObject = new GameObject(item.ItemData.ItemName + "View");
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
            if (gameObject.activeInHierarchy)
            {
                CreateItemView(item, playerInventoryGrid.ItemsParent);
                UpdateItemView(item);
            }
        }
    }

    public bool AddItem(BaseItemInstance item)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot.CanAcceptItem(item) && slot.AddItem(item))
            {
                if (gameObject.activeInHierarchy)
                {
                    CreateItemView(item, slot.transform);
                }
                return true;
            }
        }

        if (playerInventoryGrid.AddItem(item))
        {
            if (gameObject.activeInHierarchy)
            {
                CreateItemView(item, playerInventoryGrid.ItemsParent);
            }
            Debug.Log("item added + " + item.ItemData.ItemName + item.GridX + item.GridY);
            return true;
        }

        Debug.Log("item not added + " + item.ItemData.ItemName + item.GridX + item.GridY);

        return false;
    }

    public int GetItemViewCount()
    {
        return itemViews.Values.Count;
    }
}

public interface IItemInventory
{
    bool AddItem(BaseItemInstance item);
}