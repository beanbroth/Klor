using UnityEngine;

public class ArmorInstance : BaseItemInstance
{
    public float CurrentDurability { get; private set; }

    public ArmorInstance(ArmorItemData itemData, int x, int y) : base(itemData, x, y)
    {
        CurrentDurability = itemData.maxDurability;
    }

    public void EquipOrUnequip()
    {
        // Implement equip/unequip logic
        Debug.Log($"Equipping or unequipping {ItemData.ItemName}");
    }

    public void UpdateDurability(float amount)
    {
        CurrentDurability = Mathf.Clamp(CurrentDurability + amount, 0, ((ArmorItemData)ItemData).maxDurability);
    }
}
