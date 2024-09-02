using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New AttachmentItemData", menuName = "Inventory/AttachmentItemData")]
public class AttachmentItemData : BaseItemData
{
    [System.Serializable]
    public class StatModifier
    {
        public string statName;
        public float modifierPercent;
    }

    public List<StatModifier> statModifiers = new List<StatModifier>();
    public GameObject prefab;
    public AttachmentType attachmentType;

    public override BaseItemInstance CreateInstance(int x, int y)
    {
        return new AttachmentInstance(this, x, y);
    }
}
