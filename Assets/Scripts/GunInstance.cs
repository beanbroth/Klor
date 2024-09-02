using UnityEngine;
using System.Collections.Generic;

public class GunInstance : BaseItemInstance
{
    public float CurrentDurability { get; private set; }
    public float WeaponXP { get; private set; }
    public List<AttachmentInstance> Attachments { get; private set; }
    public List<OilInstance> AppliedOils { get; private set; }
    public int currentAmmo;


    public GunInstance(GunItemData itemData, int x, int y) : base(itemData, x, y)
    {
        CurrentDurability = itemData.maxDurability;
        WeaponXP = 0f;
        Attachments = new List<AttachmentInstance>();
        AppliedOils = new List<OilInstance>();
        currentAmmo = itemData.ammoCapacity;
    }

    public void AddAttachment(AttachmentInstance attachment)
    {
        Attachments.Add(attachment);
        // Apply attachment effects
    }

    public void ApplyOil(OilInstance oil)
    {
        AppliedOils.Add(oil);
        // Apply oil effects
    }

    public void UpdateDurability(float amount)
    {
        //update durability
        CurrentDurability += amount;
        if (CurrentDurability <= 0)
        {
           //break item
        }
    }

    public void AddXP(float amount)
    {
        WeaponXP += amount;
        //level up
    }
    
}