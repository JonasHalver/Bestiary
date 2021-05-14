using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Text;

public class CombatLogCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public CombatLogInformation info;
    public TextMeshProUGUI text;
    public GameObject topLine, botLine;
    public Image userIcon, victimIcon, effectIcon, buffIcon;

    public CombatAction ca;
    private Entry entry, victimEntry;
    public Color enemy, ally;
    public Image background;
    private Character user, victim;
    public bool wasStunned = false;
    public static bool displayingPast = false;
    private int t = 0;
    private Log elements;

    private void Update()
    {
        if (t > 20) { CreateCard(); }
        t++;
    }
    private void OnDisable()
    {
        if (displayingPast)
        {
            displayingPast = false;
            CombatManager.instance.ShowCurrentPositions();
        }
    }
    public void CreateCard()
    {
        elements = GameManager.instance.logElementCollection;
        OutputInfo primaryMove = null, secondaryMove = null;
        entry = ca.origin.entry;
        if (entry.isMerc) background.color = ally;
        else background.color = enemy;

        if (info.action.isPass)
        {           
            string failedText = wasStunned ? "was <sprite=21> stunned" : "passed";
            if (ca.origin.entry.isMerc) text.text = $"<b>{ca.origin.stats.characterName}</b> {failedText}.";
            else text.text = $"<b>{(ca.origin.entry.guess.characterName != null ? ca.origin.entry.guess.characterName : "The Unknown Monster")}</b> {failedText}.";
            UpdateCanvas();
            return;
        }
        #region Old Code
        /* Rewrite
        user = ca.origin;
        victim = ca.affectedCharacters.Count == 1 ? ca.affectedCharacters[0] : null;
        string username;
        if (user.stats.characterType == CharacterStats.CharacterTypes.NPC)
        {
            GetComponent<Image>().color = enemy;
            for (int i = 0; i < Book.monsterEntries.Count; i++)
            {
                if (Book.monsterEntries[i].origin.characterCode.Equals(user.stats.characterCode))
                {
                    entry = Book.monsterEntries[i];
                    break;
                }
            }
            username = entry.guess.characterName;
            userName.text = username + "'s";
            userIcon.sprite = user.stats.characterIcon;
            userIcon.color = user.stats.characterIconColor;
            Action guessAction = null;
            for (int i = 0; i < entry.actionChecks.Count; i++)
            {
                if (entry.actionChecks[i].originalAction != null)
                {
                    if (entry.actionChecks[i].originalAction.actionCode.Equals(ca.action.actionCode))
                    {
                        guessAction = entry.actionChecks[i].guessAction;
                    }
                }
            }
            if (guessAction != null) actionName.text = guessAction.actionName;
            else actionName.text = "unknown action";
        }
        else
        {
            GetComponent<Image>().color = ally;
            username = user.stats.characterName;
            userName.text = username + "'s";
            userIcon.sprite = user.stats.characterIcon;
            userIcon.color = user.stats.characterIconColor;
            actionName.text = ca.action.actionName;
        }
        if (ca.action.isPass)
        {
            Pass();
            return;
        }
        switch (ca.action.actionType)
        {
            case Action.ActionType.Attack:
                affected.text = (ca.action.isCritical ? "<b>critically</b> " : "")+"hit";
                andText.gameObject.SetActive(false);
                buffIcon.gameObject.SetActive(false);
                buffText.gameObject.SetActive(false);

                effectIcon.sprite = GameManager.Icon(ca.action.damageType);
                effectText.text = ca.action.damageType.ToString() + " damage";
                break;
            case Action.ActionType.AttackDebuff:
                affected.text = (ca.action.isCritical ? "critically " : "") + "hit";
                buffIcon.sprite = ca.action.debuff.icon;
                buffIcon.color = ca.action.debuff.iconColor;
                buffText.text = ca.action.debuff.debuffName;

                effectIcon.sprite = GameManager.Icon(ca.action.damageType);
                effectText.text = ca.action.damageType.ToString() + " damage";
                break;
            case Action.ActionType.Buff:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.sprite = ca.action.buff.icon;
                buffIcon.color = ca.action.buff.iconColor;
                buffText.text = ca.action.buff.buffName;

                effectIcon.gameObject.SetActive(false);
                effectText.gameObject.SetActive(false);
                break;
            case Action.ActionType.Debuff:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.sprite = ca.action.debuff.icon;
                buffIcon.color = ca.action.debuff.iconColor;
                buffText.text = ca.action.debuff.debuffName;

                effectIcon.gameObject.SetActive(false);
                effectText.gameObject.SetActive(false);
                break;
            case Action.ActionType.Healing:
                affected.text = "affected";
                andText.gameObject.SetActive(false);
                buffIcon.gameObject.SetActive(false);
                buffText.gameObject.SetActive(false);

                effectIcon.sprite = GameManager.Icon(Character.DamageTypes.Healing);
                effectText.text = "healing";
                break;
            case Action.ActionType.HealingBuff:
                affected.text = "affected";
                buffIcon.sprite = ca.action.buff.icon;
                buffIcon.color = ca.action.buff.iconColor;
                buffText.text = ca.action.buff.buffName;

                effectIcon.sprite = GameManager.Icon(Character.DamageTypes.Healing);
                effectText.text = "healing";
                break;
        }
        if (victim != null)
        {
            if (victim.currentBuffs.Contains(Buff.BuffType.Dodge))
            {
                if (ca.action.actionType != Action.ActionType.Healing && ca.action.actionType != Action.ActionType.HealingBuff &&
                    ca.action.actionType != Action.ActionType.Buff)
                {
                    affected.text = "missed";
                }
            }
        }

        if (victim == null)
        {
            
            victimIcon.gameObject.SetActive(false);
            victimNameText.gameObject.SetActive(false);
            xCharacters.text = $"{ca.affectedCharacters.Count.ToString()} characters";
        }
        else
        {
            string victimName;
            if (victim.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
            {
                victimName = victim.stats.characterName;
            }
            else
            {
                for (int i = 0; i < Book.monsterEntries.Count; i++)
                {
                    if (Book.monsterEntries[i].origin.characterCode.Equals(victim.stats.characterCode))
                    {
                        victimEntry = Book.monsterEntries[i];
                        break;
                    }
                }
                victimName = victimEntry.guess.characterName;
            }
            victimNameText.text = victimName;
            victimIcon.sprite = victim.stats.characterIcon;
            victimIcon.color = victim.stats.characterIconColor;
            xCharacters.gameObject.SetActive(false);
        }
        UpdateCanvas();
        */
        #endregion
        Action guessAction = null;
        if (!entry.isMerc)
        {
            if (entry.CheckByOrigin(ca.action) != null) guessAction = entry.CheckByOrigin(ca.action).guessAction;
        }

        StringBuilder sb = new StringBuilder();
        if (entry.isMerc) sb.Append($"<b>{ca.origin.stats.characterName}</b>'s <i>{ca.action.actionName}</i> ");
        else sb.Append($"<b>{(ca.origin.entry.guess.characterName != null ? ca.origin.entry.guess.characterName : "The Unknown Monster")}</b>'s <i>{(guessAction != null ? guessAction.actionName : "Unknown Action")}</i> ");

        if (info.primaryAffected.Count > 0)
        {
            sb.Append("affected ");

            if (info.primaryAffected.Count > 1)
                sb.Append($"{info.primaryAffected.Count} characters, ");
            else if (info.primaryAffected.Count == 1 && info.primaryAffected[0].stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                sb.Append($"{(info.primaryAffected[0] != ca.origin ? info.primaryAffected[0].stats.characterName : "itself")}, ");
            else if (info.primaryAffected.Count == 1 && info.primaryAffected[0].stats.characterType != CharacterStats.CharacterTypes.Adventurer)
                sb.Append($"{(info.primaryAffected[0] != ca.origin ? (info.primaryAffected[0].entry.guess.characterName != null ? info.primaryAffected[0].entry.guess.characterName : "the Unknown Monster"):"itself")}, ");
        }
        else
        {
            sb.Append("missed entirely.");
            UpdateCanvas();
            return;
        }

        bool flag1 = false;
        bool flag2 = false;

        if (info.action.primaryOutput.Count > 0)
        {
            info.action.primaryOutput.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
        if (info.action.secondaryOutput.Count > 0)
        {
            info.action.secondaryOutput.Sort((g1, g2) => g1.output.CompareTo(g2.output));
        }
        if (info.action.primaryOutput.Count > 0)
        {
            if (info.action.primaryOutput.Count == 1 && info.action.primaryOutput[0].output == Action.Output.Movement)
            {
                //sb.Append("target ");
                flag1 = true;
            }
            else if (info.action.primaryOutput.Count > 1 && info.action.primaryOutput[info.action.primaryOutput.Count - 1].output == Action.Output.Movement)
            {
                flag1 = true;
            }
            for (int i = 0; i < info.action.primaryOutput.Count; i++)
            {
                if (!flag1)
                {
                    if (i != 0 && i == info.action.primaryOutput.Count - 1) sb.Append("and ");
                }
                else
                {
                    if (i != 0 && i == info.action.primaryOutput.Count - 2) sb.Append("and ");
                }
                switch (info.action.primaryOutput[i].output)
                {
                    case Action.Output.Damage:
                        sb.Append(elements.GetString(info.action.primaryOutput[i].output, info.action.primaryOutput[i].damageType, info.action.primaryOutput[i].critical));
                        break;
                    case Action.Output.Healing:
                        sb.Append(elements.GetString(info.action.primaryOutput[i].output, info.action.primaryOutput[i].value));
                        break;
                    case Action.Output.Condition:
                        sb.Append(elements.GetString(info.action.primaryOutput[i].output, info.action.primaryOutput[i].condition, info.action.primaryOutput[i].value));
                        break;
                    case Action.Output.Movement:
                        primaryMove = info.action.primaryOutput[i];
                        break;
                }
                if (!flag1)
                {
                    if (i != info.action.primaryOutput.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        sb.Append(" to them");
                    }
                }
                else
                {
                    if (i != info.action.primaryOutput.Count - 2 && i != info.action.primaryOutput.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    else if (i != info.action.primaryOutput.Count - 1)
                    {
                        sb.Append(" to them, ");
                    }
                }
            }
        }

        if (primaryMove != null)
        {
            sb.Append(" ");

            sb.Append(elements.GetString(primaryMove.output, primaryMove.value, primaryMove.towards));
        }
        sb.Append(".");
        if (info.action.secondaryOutput.Count > 0)
        {
            sb.Append(" It also affected ");
            if (info.secondaryAffected.Count > 1)
                sb.Append($"{info.secondaryAffected.Count} characters, ");
            else if (info.secondaryAffected.Count == 1 && info.secondaryAffected[0].stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                sb.Append($"{(info.secondaryAffected[0] != ca.origin ? info.secondaryAffected[0].stats.characterName : "itself")}, ");
            else if (info.secondaryAffected.Count == 1 && info.secondaryAffected[0].stats.characterType != CharacterStats.CharacterTypes.Adventurer)
                sb.Append($"{(info.secondaryAffected[0] != ca.origin ? (info.secondaryAffected[0].entry.guess.characterName != null ? info.secondaryAffected[0].entry.guess.characterName : "the Unknown Monster"): "itself")}, ");
            if (info.action.secondaryOutput.Count == 1 && info.action.secondaryOutput[0].output == Action.Output.Movement)
            {
                //sb.Append("target ");
                flag2 = true;
            }
            else if (info.action.secondaryOutput.Count > 1 && info.action.secondaryOutput[info.action.secondaryOutput.Count - 1].output == Action.Output.Movement)
            {
                flag2 = true;
            }
            for (int i = 0; i < info.action.secondaryOutput.Count; i++)
            {
                if (!flag2)
                {
                    if (i != 0 && i == info.action.secondaryOutput.Count - 1) sb.Append("and ");
                }
                else
                {
                    if (i != 0 && i == info.action.secondaryOutput.Count - 2) sb.Append("and ");
                }
                switch (info.action.secondaryOutput[i].output)
                {
                    case Action.Output.Damage:
                        sb.Append(elements.GetString(info.action.secondaryOutput[i].output, info.action.secondaryOutput[i].damageType, info.action.secondaryOutput[i].critical));
                        break;
                    case Action.Output.Healing:
                        sb.Append(elements.GetString(info.action.secondaryOutput[i].output, info.action.secondaryOutput[i].value));
                        break;
                    case Action.Output.Condition:
                        sb.Append(elements.GetString(info.action.secondaryOutput[i].output, info.action.secondaryOutput[i].condition, info.action.secondaryOutput[i].value));
                        break;
                    case Action.Output.Movement:
                        secondaryMove = info.action.secondaryOutput[i];
                        break;
                }
                if (!flag2)
                {
                    if (i != info.action.secondaryOutput.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    else
                    {
                        sb.Append("");
                    }
                }
                else
                {
                    if (i != info.action.secondaryOutput.Count - 2 && i != info.action.secondaryOutput.Count - 1)
                    {
                        sb.Append(", ");
                    }
                    else if (i != info.action.secondaryOutput.Count - 1)
                    {
                        sb.Append(" to them,");
                    }
                }
            }
            if (secondaryMove != null)
            {
                sb.Append(" ");

                sb.Append(elements.GetString(secondaryMove.output, secondaryMove.value, secondaryMove.towards));
            }
            sb.Append(".");
        }
        string s = sb.ToString();
        s = s.Replace("deal", "dealing");
        s = s.Replace("apply", "applying");
        s = s.Replace("restore", "restoring");
        s = s.Replace("force", "forcing");
        text.text = s;
        UpdateCanvas();
    }

    public void Pass()
    {
        //victimNameText.gameObject.SetActive(false);
        //victimIcon.gameObject.SetActive(false);
        //xCharacters.gameObject.SetActive(false);
        //buffIcon.gameObject.SetActive(false);
        //buffText.gameObject.SetActive(false);
        //affected.gameObject.SetActive(false);
        //effectText.gameObject.SetActive(false);
        //andText.gameObject.SetActive(false);
        //effectIcon.gameObject.SetActive(false);
        //actionName.text = "passed.";
    }

    public void UpdateCanvas()
    {
        t = 0;

        Canvas.ForceUpdateCanvases();
    }
    public void OnPointerExit(PointerEventData eventData)
    {        
        displayingPast = false;
        CombatManager.instance.ShowCurrentPositions();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage != CombatManager.CombatStage.Setup) return;
        displayingPast = true;
        CombatManager.instance.ShowPreviousPositions(ca);
    }

    IEnumerator ExitDelay()
    {
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (displayingPast)
            {
                break;
            }
            yield return null;
        }
        if (!displayingPast)
        {
            CombatGrid.ShowCurrentPositions();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CharacterSheet.instance.ShowEntry(ca.origin);
    }

    public class CombatLogInformation
    {
        public List<Character> primaryAffected = new List<Character>();
        public List<Character> secondaryAffected = new List<Character>();
        public Action action;

        public CombatLogInformation (List<Character> pa, List<Character> sa, Action a)
        {
            primaryAffected = pa;
            secondaryAffected = sa;
            action = a;
        }
        public CombatLogInformation(List<Character> pa, Action a)
        {
            primaryAffected = pa;
            action = a;
        }
    }
}
