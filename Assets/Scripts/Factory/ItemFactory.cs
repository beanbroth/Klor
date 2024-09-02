using System;
using System.Collections.Generic;
using UnityEngine;

public interface IItemFactory
{
    BaseItemInstance CreateInstance(BaseItemData data, int x, int y);
}

public class GunFactory : IItemFactory
{
    public BaseItemInstance CreateInstance(BaseItemData data, int x, int y)
    {
        if (data is GunItemData gunData)
            return new GunInstance(gunData, x, y);
        throw new ArgumentException("Invalid data type for GunFactory");
    }
}

public class FoodFactory : IItemFactory
{
    public BaseItemInstance CreateInstance(BaseItemData data, int x, int y)
    {
        if (data is FoodItemData foodData)
            return new FoodInstance(foodData, x, y);
        throw new ArgumentException("Invalid data type for FoodFactory");
    }
}

public class ArmorFactory : IItemFactory
{
    public BaseItemInstance CreateInstance(BaseItemData data, int x, int y)
    {
        if (data is ArmorItemData armorData)
            return new ArmorInstance(armorData, x, y);
        throw new ArgumentException("Invalid data type for ArmorFactory");
    }
}

public class OilFactory : IItemFactory
{
    public BaseItemInstance CreateInstance(BaseItemData data, int x, int y)
    {
        if (data is OilItemData oilData)
            return new OilInstance(oilData, x, y);
        throw new ArgumentException("Invalid data type for OilFactory");
    }
}

public class AttachmentFactory : IItemFactory
{
    public BaseItemInstance CreateInstance(BaseItemData data, int x, int y)
    {
        if (data is AttachmentItemData attachmentData)
            return new AttachmentInstance(attachmentData, x, y);
        throw new ArgumentException("Invalid data type for AttachmentFactory");
    }
}

// Main factory class
public class ItemFactory
{
    private readonly Dictionary<Type, IItemFactory> factories;

    public ItemFactory()
    {
        factories = new Dictionary<Type, IItemFactory>
        {
            { typeof(GunItemData), new GunFactory() },
            { typeof(FoodItemData), new FoodFactory() },
            { typeof(ArmorItemData), new ArmorFactory() },
            { typeof(OilItemData), new OilFactory() },
            { typeof(AttachmentItemData), new AttachmentFactory() }
        };
    }

    public BaseItemInstance CreateItemInstance(BaseItemData data, int x, int y)
    {
        if (factories.TryGetValue(data.GetType(), out IItemFactory factory))
        {
            return factory.CreateInstance(data, x, y);
        }

        throw new ArgumentException($"No factory found for type {data.GetType()}.");
    }

    public void RegisterFactory(Type dataType, IItemFactory factory)
    {
        factories[dataType] = factory;
    }
}