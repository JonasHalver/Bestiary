using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CharacterSheetCard : MonoBehaviour, IPointerClickHandler
{
    public SimpleTooltipSpawner ttp;
    public Image iconGridBackground;
    public TextMeshProUGUI description;
    public Image icon1, icon2, icon3, icon4;
    public Entry entry;
    public Action action;
    private int priority = 0;

    public void ChangeInfo(ActionCheck actionCheck)
    {
        entry = actionCheck.entry;
        
        iconGridBackground.color = actionCheck.panelColor;
        description.text = actionCheck.guessAction.description;
        action = actionCheck.guessAction;
        
        priority = action.actionPriority;
        SetPriorityTooltip();
        switch (action.actionType)
        {
            case Action.ActionType.Attack:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = true; icon4.enabled = false;
                SetDamageTypeIcon();
                break;
            case Action.ActionType.AttackDebuff:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = true; icon4.enabled = true;
                SetDamageTypeIcon();
                SetDebuffIcon();
                break;
            case Action.ActionType.Healing:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = true; icon4.enabled = false;
                SetDamageTypeIcon();
                break;
            case Action.ActionType.HealingBuff:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = true; icon4.enabled = true;
                SetDamageTypeIcon();
                SetBuffIcon();
                break;
            case Action.ActionType.Buff:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = false; icon4.enabled = true;
                SetBuffIcon();
                break;
            case Action.ActionType.Debuff:
                icon1.enabled = true; icon2.enabled = true; icon3.enabled = false; icon4.enabled = true;
                SetDebuffIcon();
                break;
        }
        if ((!entry.isMerc && action.targetingSet) || entry.isMerc)
        {
            SimpleTooltipSpawner tts1 = icon1.gameObject.GetComponent<SimpleTooltipSpawner>(), tts2 = icon2.gameObject.GetComponent<SimpleTooltipSpawner>();
            int shapeIndex = 0;
            switch (action.shape)
            {
                case Action.Shape.All:
                    icon1.sprite = GameManager.instance.currentIconCollection.all;                    
                    icon2.enabled = false;
                    shapeIndex = 0;
                    break;
                case Action.Shape.Arc:
                    icon1.sprite = GameManager.instance.currentIconCollection.arcVert;
                    icon2.sprite = GameManager.instance.currentIconCollection.arcDia;
                    shapeIndex = 1;
                    break;
                case Action.Shape.Cone:
                    icon1.sprite = GameManager.instance.currentIconCollection.coneVert;
                    icon2.sprite = GameManager.instance.currentIconCollection.coneDia;
                    shapeIndex = 2;
                    break;
                case Action.Shape.Line:
                    icon1.sprite = GameManager.instance.currentIconCollection.lineVert;
                    icon2.sprite = GameManager.instance.currentIconCollection.lineDia;
                    shapeIndex = 3;
                    break;
                case Action.Shape.Single:
                    if (action.targetConditions.Contains(Action.Status.InMelee))
                    {
                        icon1.sprite = GameManager.instance.currentIconCollection.meleeVert;
                        icon2.sprite = GameManager.instance.currentIconCollection.meleeDia;
                        shapeIndex = 4;
                    }
                    else if (action.target == Action.Target.Self)
                    {
                        icon1.sprite = GameManager.instance.currentIconCollection.self;
                        icon2.enabled = false;
                        shapeIndex = 5;
                    }
                    else
                    {
                        icon1.sprite = GameManager.instance.currentIconCollection.ranged;
                        icon2.enabled = false;
                        shapeIndex = 6;
                    }
                    break;
                case Action.Shape.ThreeByThree:
                    if (action.target == Action.Target.Self)
                    {
                        icon1.sprite = GameManager.instance.currentIconCollection.selfBox;
                        icon2.enabled = false;
                        shapeIndex = 7;
                    }
                    else
                    {
                        icon1.sprite = GameManager.instance.currentIconCollection.box;
                        icon2.enabled = false;
                        shapeIndex = 8;
                    }
                    break;
            }
            tts1.tooltipString = GameManager.instance.currentTooltipCollection.ShapeString(shapeIndex);
            tts2.tooltipString = GameManager.instance.currentTooltipCollection.ShapeString(shapeIndex);
        }
        else
        {
            icon1.enabled = false; icon2.enabled = false;
        }
    }

    private void SetDamageTypeIcon()
    {
        SimpleTooltipSpawner tts3 = icon3.GetComponent<SimpleTooltipSpawner>();
        if ((!entry.isMerc && action.outcomeSet) || entry.isMerc)
            icon3.sprite = GameManager.Icon(action.damageType);
        else
            icon3.enabled = false;
        tts3.tooltipString = GameManager.instance.currentTooltipCollection.DamageTypeString(action.damageType);
    }
    private void SetBuffIcon()
    {
        SimpleTooltipSpawner tts4 = icon4.GetComponent<SimpleTooltipSpawner>();
        if ((!entry.isMerc && action.outcomeSet) || entry.isMerc)
        {
            icon4.sprite = action.buff.icon;
            icon4.color = action.buff.iconColor;
        }
        else
        {
            icon4.enabled = false;
        }
        tts4.tooltipString = action.buff.tooltipString;
    }
    private void SetDebuffIcon()
    {
        SimpleTooltipSpawner tts4 = icon4.GetComponent<SimpleTooltipSpawner>();
        if ((!entry.isMerc && action.outcomeSet) || entry.isMerc)
        {
            icon4.sprite = action.debuff.icon;
            icon4.color = action.debuff.iconColor;
        }
        else
        {
            icon4.enabled = false;
        }
        tts4.tooltipString = action.debuff.tooltipString;
    }
    public void SetPriorityTooltip()
    {
        string tooltipString = null;
        switch (priority)
        {
            case 1:
                tooltipString = "This action has first priority, and will be checked first.";
                break;
            case 2:
                tooltipString = "This action has second priority, and will only be checked if the first action has no valid targets.";
                break;
            case 3:
                tooltipString = "This action has third priority, and will only be checked if the previous actions had no valid targets.";
                break;
            case 4:
                tooltipString = "This action has fourth priority, and will only be checked if all other actions have no valid targets.";
                break;
        }
        ttp.tooltipString = tooltipString;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CharacterSheet.instance.ShowEntry(entry);
    }
}
