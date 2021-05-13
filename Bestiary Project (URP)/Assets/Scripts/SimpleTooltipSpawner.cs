using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SimpleTooltipSpawner : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipPrefab;
    public string tooltipString;
    private GameObject tooltip;

    public Tooltips.TooltipType type = Tooltips.TooltipType.Custom;

    public Character.DamageTypes damageType;
    public Action.Condition condition;
    public Action.Shape shape;
    public Action.TargetPriority priority;

    private ActionNode node;
    private void Awake()
    {
        node = GetComponent<ActionNode>();
        if (node)
        {
            type = Tooltips.TooltipType.Custom;
            tooltipString = $"<b><i>{node.nodeType}</i></b>" + System.Environment.NewLine + $"<b>{node.nodeName}:</b> {node.nodeDescription}";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (type)
        {
            case Tooltips.TooltipType.Custom:
                break;
            case Tooltips.TooltipType.DamageType:
                tooltipString = GameManager.instance.currentTooltipCollection.GetString(damageType);
                break;
            case Tooltips.TooltipType.Shape:
                tooltipString = GameManager.instance.currentTooltipCollection.GetString(shape);
                break;
            case Tooltips.TooltipType.Condition:
                tooltipString = GameManager.instance.currentTooltipCollection.GetString(condition);
                break;
        }
        tooltip = Instantiate(tooltipPrefab);
        SimpleTooltip stt = tooltip.GetComponent<SimpleTooltip>();
        stt.tooltipString = tooltipString;
        stt.spawn = this;
    }
    
    private void OnDisable()
    {
        if (tooltip != null)
            Destroy(tooltip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            Destroy(tooltip);
    }

    public void RefreshTooltip()
    {
        Destroy(tooltip);
        OnPointerEnter(new PointerEventData(EventSystem.current));
    }
}
