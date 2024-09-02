using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OilInstance : BaseItemInstance
{
    public OilInstance(OilItemData itemData, int x, int y) : base(itemData, x, y)
    {
    }

    // Oils don't have a right-click action, so we don't need to implement any additional methods
}