using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ContextEdit : MonoBehaviour
{
    public static ContextEdit instance;
    public ActionNode node;
    [SerializeField] private GameObject damageTypeGrid, conditionGrid;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    private Action.Condition condition = Action.Condition.None;
    private Character.DamageTypes damageType = Character.DamageTypes.None;
    public List<Button> conditionButtons = new List<Button>();
    public List<Button> damageButtons = new List<Button>();

    private bool changesMade = false;

    public void ShowEditing()
    {
        instance = this;
        title.text = node.nodeName;
        description.text = node.nodeDescription;
        if (node.nodeType == ActionNode.NodeType.Shape) return;
        for (int i = 0; i < conditionButtons.Count; i++)
        {
            SimpleTooltipSpawner sts = conditionButtons[i].GetComponent<SimpleTooltipSpawner>();
            if (sts)
            {
                Icons.Properties ip = GameManager.instance.currentIconCollection.GetIcon(conditionButtons[i].GetComponent<SimpleTooltipSpawner>().condition);
                conditionButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = ip.icon;
                conditionButtons[i].transform.GetChild(0).GetComponent<Image>().color = ip.iconColor;
            }
        }
        for (int i = 0; i < damageButtons.Count; i++)
        {
            SimpleTooltipSpawner sts = damageButtons[i].GetComponent<SimpleTooltipSpawner>();
            if (sts)
            {
                Icons.Properties ip = GameManager.instance.currentIconCollection.GetIcon(damageButtons[i].GetComponent<SimpleTooltipSpawner>().damageType);
                damageButtons[i].transform.GetChild(0).GetComponent<Image>().sprite = ip.icon;
                damageButtons[i].transform.GetChild(0).GetComponent<Image>().color = ip.iconColor;
            }
        }
        switch (node.actionContext.context)
        {
            default: break;
            case Action.Context.AllyHasSpecificCondition:
            case Action.Context.EnemyHasSpecificCondition:
            case Action.Context.ReceivedSpecificCondition:
            case Action.Context.SelfHasSpecificCondition:
                conditionGrid.SetActive(true);
                if (node.actionContext.condition != Action.Condition.None)
                {
                    foreach (Button b in conditionButtons)
                    {
                        if (b.GetComponent<SimpleTooltipSpawner>() && b.GetComponent<SimpleTooltipSpawner>().condition == node.actionContext.condition) 
                        {
                            b.interactable = false;
                            condition = node.actionContext.condition;
                        }
                    }
                }
                break;
            case Action.Context.TookDamageOfType:
                damageTypeGrid.SetActive(true);
                if (node.actionContext.damageType != Character.DamageTypes.None)
                {
                    foreach (Button b in damageButtons)
                    {
                        if (b.GetComponent<SimpleTooltipSpawner>() && b.GetComponent<SimpleTooltipSpawner>().damageType == node.actionContext.damageType)
                        {
                            b.interactable = false;
                            damageType = node.actionContext.damageType;
                        }
                    }
                }
                break;
        }
    }

    public void SetDamageType(Character.DamageTypes dt)
    {
        damageType = dt;
        changesMade = true;
    }
    public void SetCondition(Action.Condition c)
    {
        condition = c;
        changesMade = true;
    }
    public void CloseWindow()
    {
        node.actionContext.condition = condition;
        node.actionContext.damageType = damageType;
        node.EndEdit(changesMade);
        Destroy(gameObject);
    }
}
