using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookActionOutcome : MonoBehaviour
{
    public TextMeshProUGUI title;
    public TMP_Dropdown dealDamage, buffOrDebuff, buffType, debuffType, targetGroup, critical, damageType;
    // Possibly set the contents on these dropdowns based on lists filled out in the inspector, for futureproofing
    public List<Buff> buffs = new List<Buff>();
    public List<Debuff> debuffs = new List<Debuff>();
    private void OnEnable()
    {

        SetActionType();
        SetBuffOrDebuff();
        SetTargetGroup();
        SetDamageType();
        SetCritical();
        Book.currentEntry.activeAction.guessAction.outcomeSet = true;
    }

    private void Update()
    {
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "the monster";
        titleText = titleText.Replace("*", replacement);
        title.text = titleText;


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
        SetDamageType();
        SetCritical();
        Book.currentEntry.activeAction.CalculateValidity();
        Book.currentEntry.activeAction.guessAction.outcomeSet = true;
        BookActionCard.CardUpdate();
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
        switch (dealDamage.value)
        {
            case 0:
                critical.interactable = true;
                damageType.interactable = true;
                break;
            case 1:
                if (buffOrDebuff.value == 0)
                {
                    if (targetGroup.value == 1)
                    {
                        buffOrDebuff.value = 1;
                    }
                    else
                    {
                        buffOrDebuff.value = 2;
                    }
                }
                critical.interactable = false;
                critical.value = 0;
                damageType.interactable = false;
                damageType.value = damageType.options.Count - 1;
                break;
            case 2:
                critical.interactable = false;
                critical.value = 0;
                damageType.interactable = false;
                damageType.value = damageType.options.Count - 1;
                break;
        }
        switch (buffOrDebuff.value)
        {
            case 0:
                buffType.interactable = false;
                buffType.value = buffType.options.Count - 1;
                debuffType.interactable = false;
                debuffType.value = debuffType.options.Count - 1;
                break;
            case 1:
                buffType.interactable = true;
                if (buffType.value == buffType.options.Count - 1) buffType.value = 0;
                debuffType.interactable = false;
                debuffType.value = debuffType.options.Count - 1;
                break;
            case 2:
                buffType.interactable = false;
                buffType.value = buffType.options.Count - 1;
                debuffType.interactable = true;
                if (debuffType.value == debuffType.options.Count - 1) debuffType.value = 0;
                break;
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
                if (buffType.value != buffType.options.Count - 1)
                    action.buff = buffs[buffType.value];
                break;
            case 2:
                if (debuffType.value != debuffType.options.Count-1)
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

    public void SetDamageType()
    {
        Action action = Book.currentEntry.activeAction.guessAction;
        if (damageType.value != damageType.options.Count-1)
            action.damageType = (Character.DamageTypes)damageType.value;
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
