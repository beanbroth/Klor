using UnityEngine;

[CreateAssetMenu(fileName = "New FoodItemData", menuName = "Inventory/FoodItemData")]
public class FoodItemData : BaseItemData
{
    public float healthRestored;
    public float healthRestoreTime;

    public override BaseItemInstance CreateInstance(int x, int y)
    {
        return new FoodInstance(this, x, y);
    }
}