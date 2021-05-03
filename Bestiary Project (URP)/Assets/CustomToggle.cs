using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CustomToggle : MonoBehaviour, IPointerClickHandler
{
    public Image img;
    public TextMeshProUGUI text;
    public List<Sprite> sprites = new List<Sprite>();
    public List<string> tooltips = new List<string>();
    private SimpleTooltipSpawner sts;
    [SerializeField]private int activeIndex = 0;
    public ActionNode.ToggleType type;
    public bool primary; 

    public void OnPointerClick(PointerEventData eventData)
    {
        activeIndex++;
        if (activeIndex == sprites.Count) activeIndex = 0;
        if (img) img.sprite = sprites[activeIndex];
        else if (text) text.text = (activeIndex + 1).ToString();
        sts.tooltipString = tooltips[activeIndex];
        sts.RefreshTooltip();
        switch (type)
        {
            case ActionNode.ToggleType.Cooldown:
                ActionEditor.instance.SetCooldown(activeIndex);
                break;
            case ActionNode.ToggleType.TargetGroup:
                ActionEditor.instance.SetTargetGroup(activeIndex, primary);
                break;
            case ActionNode.ToggleType.Targeting:
                ActionEditor.instance.SetTargeting(activeIndex, primary);
                break;
        }
    }

    private void Awake()
    {
        sts = GetComponent<SimpleTooltipSpawner>();
        sts.tooltipString = tooltips[0];
    }
}
