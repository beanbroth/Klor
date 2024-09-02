using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New GunItemData", menuName = "Inventory/GunItemData")]
public class GunItemData : BaseItemData
{
    [Header("Gun Properties")]
    public GameObject prefab;
    public float maxDurability = 100f;
    public float damage = 10f;
    public float fireRate = 1f;
    public bool isAutomatic = false;
    public float bulletSpeed = 100f;
    public float inaccuracy = 0.1f;
    public float recoilPerShot = 1f;
    public int bulletsPerShot = 1;
    public int ammoCapacity = 30;
    public GameObject bulletPrefab;


    //public new EquipmentSlotType equipmentSlotType = EquipmentSlotType.Weapon;
    
    [Header("Attachment Slots")]
    public List<AttachmentSlot> attachmentSlots = new List<AttachmentSlot>();


    public override BaseItemInstance CreateInstance(int x, int y)
    {
        
        return new GunInstance(this, x, y);
    }
}

[System.Serializable]
public class AttachmentSlot
{
    public string slotName;
    public AttachmentType allowedType;
}

public enum AttachmentType
{
    Scope,
    Laser,
    Grip
}