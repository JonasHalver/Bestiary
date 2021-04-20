using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class OutputEdit : MonoBehaviour
{
    public static OutputEdit instance;
    public ActionNode node;
    [SerializeField] private GameObject damageTypeGrid, conditionGrid, criticalToggle, valueInput, towardsAwayToggle;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    private Action.Condition condition = Action.Condition.None;
    private Character.DamageTypes damageType = Character.DamageTypes.None;
    public List<Button> conditionButtons = new List<Button>();
    public List<Button> damageButtons = new List<Button>();
    public Button towardsButton, awayButton;
    public Toggle crit;
    public TMP_InputField input;
    private bool critical = false;
    private bool towards = false;
    private int value = 0;

    private bool changesMade = false;

    public void ShowEditing()
    {
        instance = this;
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
        title.text = node.nodeName;
        description.text = node.nodeDescription;
        switch (node.actionOutput.output)
        {
            case Action.Output.Damage:
                damageTypeGrid.SetActive(true);
                criticalToggle.SetActive(true);
                crit.isOn = node.actionOutput.critical;
                if (node.actionOutput.damageType != Character.DamageTypes.None)
                {
                    foreach (Button b in damageButtons)
                    {
                        if (b.GetComponent<SimpleTooltipSpawner>() && b.GetComponent<SimpleTooltipSpawner>().damageType == node.actionOutput.damageType)
                        {
                            b.interactable = false;
                        }
                    }
                }
                break;
            case Action.Output.Healing:
                valueInput.SetActive(true);
                damageType = Character.DamageTypes.Healing;
                input.text = node.actionOutput.value.ToString();
                break;
            case Action.Output.Condition:
                conditionGrid.SetActive(true);
                valueInput.SetActive(true);
                if (node.actionOutput.condition != Action.Condition.None)
                {
                    foreach (Button b in conditionButtons)
                    {
                        if (b.GetComponent<SimpleTooltipSpawner>() && b.GetComponent<SimpleTooltipSpawner>().condition == node.actionOutput.condition)
                        {
                            b.interactable = false;
                        }
                    }
                }
                input.text = node.actionOutput.value.ToString();
                break;
            case Action.Output.Movement:
                valueInput.SetActive(true);
                towardsAwayToggle.SetActive(true);
                towardsButton.interactable = !node.actionOutput.towards;
                awayButton.interactable = node.actionOutput.towards;
                input.text = node.actionOutput.value.ToString();
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
    public void SetTowardsAway(bool toward)
    {
        towards = toward;
        changesMade = true;
    }
    public void SetCritical()
    {
        critical = crit.isOn;
    }
    public void SetValue()
    {
        value = int.Parse(input.text);
        changesMade = true;
    }

    public void CloseWindow()
    {
        node.actionOutput.condition = condition;
        node.actionOutput.damageType = damageType;
        node.actionOutput.critical = critical;
        node.actionOutput.towards = towards;
        node.actionOutput.value = value;
        node.EndEdit(changesMade);
        Destroy(gameObject);
    }
}
