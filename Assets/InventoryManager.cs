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

    [SerializeField] private S_InventoryGrid playerInventoryGrid;
    [SerializeField] private List<EquipmentSlot> equipmentSlots = new List<EquipmentSlot>();
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
        // Store the original parent before attempting to move the item
        Transform originalParent = itemViews[item].transform.parent;

        // Try to add the item to an equipment slot
        foreach (var slot in equipmentSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.GetComponent<RectTransform>(), position))
            {
                if (slot.CanAcceptItem(item) && slot.AddItem(item))
                {
                    RemoveItemFromPreviousLocation(item, slot.transform);
                    UpdateItemViewParent(item, slot.transform);
                    UpdateItemView(item);
                    return true;
                }
                return false;
            }
        }

        // Try to add the item to the grid inventory
        if (playerInventoryGrid.HandleItemDrop(item, position))
        {
            RemoveItemFromPreviousLocation(item, playerInventoryGrid.ItemsParent);
            UpdateItemViewParent(item, playerInventoryGrid.ItemsParent);
            return true;
        }

        // If we reach here, the drop failed, so we keep the item in its original location
        UpdateItemViewParent(item, originalParent);
        return false;
    }

    private void RemoveItemFromPreviousLocation(BaseItemInstance item, Transform newParent)
    {
        // Check if the item is moving to a different parent
        if (itemViews[item].transform.parent != newParent)
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
        // If the item is staying in the same parent (e.g., moving within the grid),
        // we don't need to remove it from its previous location
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
            Debug.Log("item added + " + item.ItemData.itemName + item.GridX + item.GridY);
            return true;
        }

        Debug.Log("item not added + " + item.ItemData.itemName + item.GridX + item.GridY);

        return false;
    }
}

public interface IItemInventory
{
    bool AddItem(BaseItemInstance item);
}