using UnityEngine;
using System.Collections.Generic;

public class GunInstance : BaseItemInstance
{
    public float CurrentDurability { get; private set; }
    public float WeaponXP { get; private set; }
    public List<AttachmentInstance> Attachments { get; private set; }
    public List<OilInstance> AppliedOils { get; private set; }

    public GunInstance(GunItemData itemData, int x, int y) : base(itemData, x, y)
    {
        CurrentDurability = itemData.maxDurability;
        WeaponXP = 0f;
        Attachments = new List<AttachmentInstance>();
        AppliedOils = new List<OilInstance>();
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


    public void EquipOrUnequip()
    {
        // Implement equip/unequip logic
    }

    public void UpdateDurability(float amount)
    {
        CurrentDurability = Mathf.Clamp(CurrentDurability + amount, 0, ((GunItemData)ItemData).maxDurability);
    }

    public void AddXP(float amount)
    {
        WeaponXP += amount;
        // Implement level up logic if needed
    }

    //override right click action

}