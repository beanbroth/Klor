//using UnityEngine;

//public class WorldItem : MonoBehaviour, IInteractable
//{
//    public SO_InventoryItemData itemData;

//    public void Interact(PlayerInteractController player)
//    {
//        GameObject itemObject = new GameObject(itemData.itemName);
//        S_InventoryItem newItem = itemObject.AddComponent<S_InventoryItem>();
//        newItem.Initialize(itemData, 0, 0, player.playerInventory.CellSize);

//        if (player.AddItemToInventory(newItem))
//        {
//            Debug.Log($"Picked up {itemData.itemName}");
//            Destroy(gameObject);
//        }
//        else
//        {
//            Debug.Log("Inventory is full!");
//            Destroy(itemObject);
//        }
//    }

//    public string GetInteractionPrompt()
//    {
//        return $"Press E to pick up {itemData.itemName}";
//    }
//}
