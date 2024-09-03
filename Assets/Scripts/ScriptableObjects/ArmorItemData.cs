using UnityEngine;

public enum ArmorSlot
{
    Head,
    Body,
    Foot
}

[CreateAssetMenu(fileName = "New ArmorItemData", menuName = "Inventory/ArmorItemData")]
public class ArmorItemData : BaseItemData
{
    [Space(10)]
    [Header("Armor Specific Data")]
    public float armorPercentage;
    public float maxDurability;
    public ArmorSlot armorSlot;

    public override BaseItemInstance CreateInstance(int x, int y)
    {
        return new ArmorInstance(this, x, y);
    }
}