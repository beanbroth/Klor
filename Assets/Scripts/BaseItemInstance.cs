using UnityEngine;

public class BaseItemInstance : IRightClickable
{
    public BaseItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public RotationState CurrentRotation { get; private set; }
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public bool[,] CurrentShape { get; private set; }

    public string Name => ItemData.name;

    public BaseItemInstance(BaseItemData itemData, int x, int y)
    {
        ItemData = itemData;
        GridX = x;
        GridY = y;
        CurrentRotation = RotationState.Rotation0;
        CurrentWidth = itemData.Width;
        CurrentHeight = itemData.Height;
        CurrentShape = (bool[,])itemData.Shape.Clone();
    }

    public void UpdatePosition(int newX, int newY)
    {
        GridX = newX;
        GridY = newY;
    }

    public void Rotate()
    {
        CurrentRotation = (RotationState)(((int)CurrentRotation + 1) % 4);
        UpdateShapeAndSize();
    }

    private void UpdateShapeAndSize()
    {
        switch (CurrentRotation)
        {
            case RotationState.Rotation0:
            case RotationState.Rotation180:
                CurrentWidth = ItemData.Width;
                CurrentHeight = ItemData.Height;
                break;
            case RotationState.Rotation90:
            case RotationState.Rotation270:
                CurrentWidth = ItemData.Height;
                CurrentHeight = ItemData.Width;
                break;
        }

        CurrentShape = new bool[CurrentWidth, CurrentHeight];
        for (int i = 0; i < ItemData.Width; i++)
        {
            for (int j = 0; j < ItemData.Height; j++)
            {
                switch (CurrentRotation)
                {
                    case RotationState.Rotation0:
                        CurrentShape[i, j] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation90:
                        CurrentShape[ItemData.Height - 1 - j, i] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation180:
                        CurrentShape[ItemData.Width - 1 - i, ItemData.Height - 1 - j] = ItemData.Shape[i, j];
                        break;
                    case RotationState.Rotation270:
                        CurrentShape[j, ItemData.Width - 1 - i] = ItemData.Shape[i, j];
                        break;
                }
            }
        }
    }

    public virtual void OnRightClick(InventoryContext context)
    {
        switch (context)
        {
            case InventoryContext.Shop:
                Sell();
                break;
            case InventoryContext.Default:
                PerformDefaultRightClickAction();
                break;
                // Add more cases for other contexts as needed
        }
    }

    protected virtual void Sell()
    {
        Debug.Log($"Selling {ItemData.ItemName}");

        //TODO: Implement selling logic? prolly won't have time
    }

    protected virtual void PerformDefaultRightClickAction()
    {
        // Default implementation (do nothing)
        // Child classes will override this method if they have a specific action
    }
}

public enum RotationState
{
    Rotation0,
    Rotation90,
    Rotation180,
    Rotation270
}

public interface IRightClickable
{
    void OnRightClick(InventoryContext context);
}

public enum InventoryContext
{
    Default,
    Shop,
    // Add more contexts as needed
}
