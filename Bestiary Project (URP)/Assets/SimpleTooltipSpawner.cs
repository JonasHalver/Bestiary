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

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip = Instantiate(tooltipPrefab);
        SimpleTooltip stt = tooltip.GetComponent<SimpleTooltip>();
        stt.tooltipString = tooltipString;
        stt.spawn = this;
    }

    private void OnDisable()
    {
        Destroy(tooltip);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Destroy(tooltip);
    }
}
