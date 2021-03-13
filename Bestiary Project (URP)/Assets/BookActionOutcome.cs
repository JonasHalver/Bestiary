using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookActionOutcome : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TMP_Dropdown dealDamage, buffOrDebuff, buffType, debuffType, targetGroup, critical;
    // Possibly set the contents on these dropdowns based on lists filled out in the inspector, for futureproofing
    public List<Buff> buffs = new List<Buff>();
    public List<Debuff> debuffs = new List<Debuff>();
    private void OnEnable()
    {
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.stats.characterName != "" ? Book.currentEntry.guess.stats.characterName : "the monster";
        titleText.Replace("*", replacement);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void ValueChanged()
    {
        SetActionType();
        SetBuffOrDebuff();
        SetTargetGroup();
        SetCritical();
        Book.currentEntry.activeAction.CalculateValidity();
    }

    public void SetActionType()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        bool damage = false, healing = false;

        bool debuff = false, buff = false;
        switch (dealDamage.value)
        {
            case 0:
                damage = true;
                break;
            case 1:
                damage = false;
                break;
            case 2:
                healing = true;
                break;
        }
        switch (buffOrDebuff.value)
        {
            case 0:
                debuff = false;
                buff = false;
                break;
            case 1:
                buff = true;
                break;
            case 2:
                debuff = true;
                break;
        }
        if (damage)
        {
            if (debuff) action.actionType = Action.ActionType.AttackDebuff;
            else action.actionType = Action.ActionType.Attack;
        } 
        else if (healing)
        {
            if (buff) action.actionType = Action.ActionType.HealingBuff;
            else action.actionType = Action.ActionType.Healing;
        }
        else
        {
            if (buff) action.actionType = Action.ActionType.Buff;
            else if (debuff) action.actionType = Action.ActionType.Debuff;
        }
    }
    public void SetBuffOrDebuff()
    {
        Action action = Book.currentEntry.activeAction.guessAction;

        switch (buffOrDebuff.value)
        {
            case 0:
                return;
            case 1:
                switch (buffType.value)
                {
                    case 0:
                        break;
                    case 1:

                        break;
                    case 2:
                        Buff buff = null;
                        foreach(Buff b in buffs)
                        {
                            if (b.buffName == "Dodge") buff = b;
                        }
                        action.buff = buff;
                        break;
                }
                break;
            case 2:
                action.debuff = debuffs[debuffType.value];
                break;
        }
    }
    public void SetTargetGroup()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        switch (targetGroup.value)
        {
            case 0:
                action.targetGroup = Action.TargetGroup.All;
                break;
            case 1:
                action.targetGroup = Action.TargetGroup.Allies;
                break;
            case 2:
                action.targetGroup = Action.TargetGroup.Enemies;
                break;
        }
    }
    public void SetCritical()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        switch (critical.value)
        {
            case 0:
                action.isCritical = false;
                break;
            case 1:
                action.isCritical = true;
                break;
        }
    }
}
