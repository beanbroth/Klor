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

        
        // Find and add all EquipmentSlots in the scene
        equipmentSlots.AddRange(FindObjectsOfType<EquipmentSlot>());
    }

    public bool HandleItemDrag(S_InventoryItem item, Vector2 position)
    {
        // Check if the item is over any equipment slot
        foreach (var slot in equipmentSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.GetComponent<RectTransform>(), position))
            {
                // Highlight the slot if it can accept the item
                if (slot.CanAcceptItem(item))
                {
                    // Implement highlight logic
                }
                return true;
            }
        }

        // If not over an equipment slot, handle grid drag
        return playerInventoryGrid.HandleItemDrag(item, position);
    }

    public bool HandleItemDrop(S_InventoryItem item, Vector2 position)
    {
        // Check if the item is dropped on any equipment slot
        foreach (var slot in equipmentSlots)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(slot.GetComponent<RectTransform>(), position))
            {
                if (slot.AddItem(item))
                {
                    // Remove the item from its previous location (grid or another equipment slot)
                    RemoveItemFromPreviousLocation(item);
                    return true;
                }
                return false;
            }
        }

        // If not dropped on an equipment slot, handle grid drop
        if (playerInventoryGrid.HandleItemDrop(item, position))
        {
            // Remove the item from its previous equipment slot if it was in one
            RemoveItemFromPreviousLocation(item);
            return true;
        }

        return false;
    }

    private void RemoveItemFromPreviousLocation(S_InventoryItem item)
    {
        foreach (var slot in equipmentSlots)
        {
            if (slot.RemoveItem(item))
            {
                return;
            }
        }


        //when it's dropped onto the grid, it then imediately removes itself?
        //TODO: Revist this logic once other grids are implemented (shops)
        //inventoryGrid.RemoveItem(item);
    }

    public void PlaceItem(S_InventoryItem item, int x, int y)
    {
        playerInventoryGrid.PlaceItem(item, x, y);
    }
}

public interface IInventorySlot
{
    bool CanAcceptItem(S_InventoryItem item);
    bool AddItem(S_InventoryItem item);
    bool RemoveItem(S_InventoryItem item);
    Vector2 GetSlotPosition();
}