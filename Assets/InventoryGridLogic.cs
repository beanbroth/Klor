using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.Progress;

public class InventoryGridLogic
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }

    private List<BaseItemInstance> items = new List<BaseItemInstance>();

    public InventoryGridLogic(int width, int height, float cellSize)
    {
        Width = width;
        Height = height;
        CellSize = cellSize;
    }

    public bool CanPlaceItem(BaseItemInstance item, int x, int y, BaseItemInstance ignoredItem = null)
    {
        if (x < 0 || y < 0 || x + item.CurrentWidth > Width || y + item.CurrentHeight > Height)
            return false;

        for (int i = 0; i < item.CurrentWidth; i++)
        {
            for (int j = 0; j < item.CurrentHeight; j++)
            {
                if (item.CurrentShape[i, j])
                {
                    if (IsOccupied(x + i, y + j, ignoredItem))
                        return false;
                }
            }
        }
        return true;
    }

    private bool IsOccupied(int x, int y, BaseItemInstance ignoredItem)
    {
        foreach (var item in items)
        {
            if (item == ignoredItem) continue;

            if (item.GridX <= x && x < item.GridX + item.CurrentWidth &&
                item.GridY <= y && y < item.GridY + item.CurrentHeight)
            {
                if (item.CurrentShape[x - item.GridX, y - item.GridY])
                    return true;
            }
        }
        return false;
    }

    public bool PlaceItem(BaseItemInstance item, int x, int y)
    {
        if (CanPlaceItem(item, x, y))
        {
            item.UpdatePosition(x, y);
            items.Add(item);
            return true;
        }
        return false;
    }

    public bool RemoveItem(BaseItemInstance item)
    {
        return items.Remove(item);
    }

    public bool AddItem(BaseItemInstance item)
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                if (CanPlaceItem(item, x, y))
                {
                    PlaceItem(item, x, y);
                    return true;
                }

                for (int r = 0; r < 3; r++)
                {
                    item.Rotate();
                    if (CanPlaceItem(item, x, y))
                    {
                        PlaceItem(item, x, y);
                        return true;
                    }
                }

                item.Rotate();
            }
        }
        return false;
    }

    //don't change the position, just rotate the item in place to fit
    public bool RotateObjectToFit(BaseItemInstance item)
    {

        for (int r = 0; r < 4; r++) // Try all 4 possible rotations
        {
            Debug.Log(item.GridX + item.GridY);
            if (CanPlaceItem(item, item.GridX, item.GridY, item))
            {
                return true;
            }
            item.Rotate();
        }
        return false;

    }

    public bool PlaceItemWithAnyRotation(BaseItemInstance item, int x, int y)
    {
        for (int r = 0; r < 4; r++) // Try all 4 possible rotations
        {
            if (CanPlaceItem(item, x, y))
            {
                PlaceItem(item, x, y);
                return true;
            }
            item.Rotate();
        }
        return false;
    }

    public void HandleItemRotation(BaseItemInstance item)
    {
        if (!CanPlaceItem(item, item.GridX, item.GridY, item))
        {
            item.Rotate();
        }
    }

    //get all items in the grid
    public List<BaseItemInstance> GetItems()
    {
        return items;
    }
}