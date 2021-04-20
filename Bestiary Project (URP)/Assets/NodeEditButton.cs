using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeEditButton : MonoBehaviour
{
    public ActionNode.NodeType type = ActionNode.NodeType.Context;
    public Tooltips.TooltipType info = Tooltips.TooltipType.DamageType;
    public void ButtonClicked()
    {
        SimpleTooltipSpawner sts = GetComponent<SimpleTooltipSpawner>();
        if (type == ActionNode.NodeType.Context)
        {
            if (info == Tooltips.TooltipType.DamageType)
            {
                ContextEdit.instance.SetDamageType(sts.damageType);
            }
            else if (info == Tooltips.TooltipType.Condition)
            {
                ContextEdit.instance.SetCondition(sts.condition);
            }
        }
        else if (type == ActionNode.NodeType.Output)
        {
            if (info == Tooltips.TooltipType.DamageType)
            {
                OutputEdit.instance.SetDamageType(sts.damageType);
            }
            else if (info == Tooltips.TooltipType.Condition)
            {
                OutputEdit.instance.SetCondition(sts.condition);
            }
        }
    }
}
