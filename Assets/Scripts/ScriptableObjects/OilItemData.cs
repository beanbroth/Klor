using UnityEngine;
using System.Collections.Generic;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

[CreateAssetMenu(fileName = "New OilItemData", menuName = "Inventory/OilItemData")]
public class OilItemData : BaseItemData
{
    [System.Serializable]
    public class StatModifier
    {
        public string statName;
        public float modifierPercent;
    }

    public List<StatModifier> statModifiers = new List<StatModifier>();

    public override BaseItemInstance CreateInstance(int x, int y)
    {
        return new OilInstance(this, x, y);
    }
}
