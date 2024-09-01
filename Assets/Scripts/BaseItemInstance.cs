public class BaseItemInstance
{
    public BaseItemData ItemData { get; private set; }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public RotationState CurrentRotation { get; private set; }
    public int CurrentWidth { get; private set; }
    public int CurrentHeight { get; private set; }
    public bool[,] CurrentShape { get; private set; }

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
}

public enum RotationState
{
    Rotation0,
    Rotation90,
    Rotation180,
    Rotation270
}