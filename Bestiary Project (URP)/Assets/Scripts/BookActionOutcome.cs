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
    public string titleString;
    public bool isLoading = false;
    /* Outdated
    private void OnEnable()
    {
        StartCoroutine(CardLoad());
    }

    private void Update()
    {
        string titleText = titleString;
        string cname = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "the monster";

        titleText = titleText.Replace("*", cname);
        title.text = titleText;
    }
    IEnumerator CardLoad()
    {
        isLoading = true;
        ResetValues(false);
        isLoading = true;
        yield return null;
        SetInfoFromGuess();
        yield return null;
        SetActionType();
        yield return null;
        SetBuffOrDebuff();
        yield return null;
        SetTargetGroup();
        yield return null;
        SetDamageType();
        yield return null;
        SetCritical();
        yield return null;
        Book.currentEntry.activeAction.guessAction.outcomeSet = true;
        BookActionCard.CardUpdate();

        isLoading = false;
    }
    private void SetInfoFromGuess()
    {
        Action action = Book.currentEntry.activeAction.guessAction;

        switch (action.actionType)
        {
            case Action.ActionType.Attack:
                dealDamage.value = 0;
                buffOrDebuff.value = 0;
                critical.value = action.isCritical ? 1 : 0;
                debuffType.value = debuffType.options.Count - 1;
                buffType.value = buffType.options.Count - 1;
                break;
            case Action.ActionType.AttackDebuff:
                dealDamage.value = 0;
                buffOrDebuff.value = 2;
                critical.value = action.isCritical ? 1 : 0;
                break;
            case Action.ActionType.Healing:
                dealDamage.value = 2;
                buffOrDebuff.value = 0;
                debuffType.value = debuffType.options.Count - 1;
                buffType.value = buffType.options.Count - 1;
                damageType.value = damageType.options.Count - 1;
                break;
            case Action.ActionType.HealingBuff:
                dealDamage.value = 2;
                buffOrDebuff.value = 1;
                debuffType.value = debuffType.options.Count - 1;
                damageType.value = damageType.options.Count - 1;
                break;
            case Action.ActionType.Debuff:
                dealDamage.value = 1;
                buffOrDebuff.value = 2;
                buffType.value = buffType.options.Count - 1;
                damageType.value = damageType.options.Count - 1;
                break;
            case Action.ActionType.Buff:
                dealDamage.value = 1;
                buffOrDebuff.value = 1;
                debuffType.value = debuffType.options.Count - 1;
                damageType.value = damageType.options.Count - 1;
                break;
        }
        if (action.buff != null)
        {
            for (int i = 0; i < buffs.Count; i++)
            {
                if (buffs[i].buffType == action.buff.buffType) buffType.value = i;
            }
        }
        if (action.debuff != null)
        {
            for (int i = 0; i < debuffs.Count; i++)
            {
                if (action.debuff.debuffType == Debuff.DebuffType.Control) 
                {
                    if (debuffs[i].debuffType == Debuff.DebuffType.Control && debuffs[i].controlType == action.debuff.controlType) debuffType.value = i; 
                }
                else
                {
                    if (debuffs[i].debuffType == Debuff.DebuffType.DamageOverTime && debuffs[i].damageType == action.debuff.damageType) debuffType.value = i;
                }

            }
        }
        switch (action.targetGroup)
        {
            case Action.TargetGroup.All:
                targetGroup.value = 0;
                break;
            case Action.TargetGroup.Allies:
                targetGroup.value = 1;
                break;
            case Action.TargetGroup.Enemies:
                targetGroup.value = 2;
                break;
        }
        damageType.value = (int)action.damageType;
    }

    public void ResetValues(bool hard)
    {
        isLoading = true;
        dealDamage.value = 0; buffOrDebuff.value = 0; buffType.value = 0; debuffType.value = 0; targetGroup.value = 0; critical.value = 0; damageType.value = 0;
        isLoading = false;
        if (hard)
        {
            ValueChanged();
            Book.currentEntry.activeAction.guessAction.outcomeSet = false;
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void ValueChanged()
    {
        if (isLoading) return;
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
    */
}
