using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New BaseItemData", menuName = "Inventory/BaseItemData")]
public abstract class BaseItemData : ScriptableObject
{
    [Header("Basic Item Data")]
    [SerializeField] private string itemName;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private EquipmentSlotType equipmentSlotType;
    [Range(1, 9)] public int width = 1;
    [Range(1, 9)] public int height = 1;
    public List<bool> shapeData = new List<bool>();
    public int Width => width;
    public int Height => height;

    public bool[,] Shape
    {
        get
        {
            bool[,] shape = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    if (index < shapeData.Count)
                    {
                        shape[x, y] = shapeData[index];
                    }
                }
            }

            return shape;
        }
    }

    public Sprite ItemSprite => itemSprite;
    public string ItemName => itemName;
    public EquipmentSlotType EquipmentSlotType => equipmentSlotType;

    public void SetShape(bool[,] newShape)
    {
        width = newShape.GetLength(0);
        height = newShape.GetLength(1);
        shapeData.Clear();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                shapeData.Add(newShape[x, y]);
            }
        }
    }

    // Helper method to get a specific cell's value
    public bool GetCell(int x, int y)
    {
        int index = y * width + x;
        return index < shapeData.Count ? shapeData[index] : false;
    }

    // Helper method to set a specific cell's value
    public void SetCell(int x, int y, bool value)
    {
        int index = y * width + x;
        if (index < shapeData.Count)
        {
            shapeData[index] = value;
        }
    }

    public abstract BaseItemInstance CreateInstance(int x, int y);
}

public enum EquipmentSlotType
{
    None,
    Head,
    Chest,
    Feet,
    Weapon,
    Trinket,
    Nade
}