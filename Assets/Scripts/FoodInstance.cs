using UnityEngine;

public class FoodInstance : BaseItemInstance
{
    public FoodInstance(FoodItemData itemData, int x, int y) : base(itemData, x, y)
    {
    }

    public void Consume()
    {
        // Implement food consumption logic
        // This could involve starting a coroutine to restore health over time
        Debug.Log($"Consumed {ItemData.itemName}, restoring {((FoodItemData)ItemData).healthRestored} health over {((FoodItemData)ItemData).healthRestoreTime} seconds");
    }
}
