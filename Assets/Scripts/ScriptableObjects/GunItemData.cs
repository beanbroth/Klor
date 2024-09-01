using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New GunItemData", menuName = "Inventory/GunItemData")]
public class GunItemData : BaseItemData
{
    [Header("Gun Properties")]
    public GameObject prefab;
    public float maxDurability = 100f;
    public float damagePerBullet = 10f;
    public float rateOfFire = 1f;
    public bool isAutomatic = false;
    public float bulletSpeed = 100f;
    public float inaccuracy = 0.1f;
    public float recoilPerShot = 1f;
    public int bulletsPerShot = 1;
    public int ammoCapacity = 30;

    [Header("Attachment Slots")]
    public List<AttachmentSlot> attachmentSlots = new List<AttachmentSlot>();
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