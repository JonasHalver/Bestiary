using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BookActionCard : MonoBehaviour
{
    public GameObject description, targeting, outcome;
    public TextMeshProUGUI descriptionText, targetingText, outcomeText;
    private Action guess;

    private void Start()
    {
        descriptionText = description.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        targetingText = targeting.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        outcomeText = outcome.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }
    private void Update()
    {
        guess = Book.currentEntry.activeAction.guessAction;
        SetDescriptionText();
    }
    public void OpenDescription()
    {
        description.SetActive(true);
    }
    public void SetDescriptionText()
    {
        if (guess.descriptionIndex == -1) descriptionText.text = "(action description)";
        else
        {
            descriptionText.text = Book.instance.descriptionsList.descriptions[guess.descriptionIndex];
        }
    }
    public void OpenTargeting()
    {
        targeting.SetActive(true);
    }
    public void SetTargeting()
    {
        string line = "* ";
        string target = "";
        switch (guess.targetGroup)
        {
            case Action.TargetGroup.All:
                switch (guess.targetConditions[0])
                {
                    case Action.Status.Above50:
                        target = guess.minimumHits == 1 ? "a healthy character" : "healthy characters";
                        break;
                    case Action.Status.Below50:
                        target = guess.minimumHits == 1 ? "a hurt character" : "hurt characters";
                        break;
                    case Action.Status.InMelee:
                        target = guess.minimumHits == 1 ? "a close character" : "close characters";
                        break;
                    case Action.Status.NotInMelee:
                        target = guess.minimumHits == 1 ? "a distant character" : "distant characters";
                        break;
                }
                break;
            case Action.TargetGroup.Allies:
                switch (guess.targetConditions[0])
                {
                    case Action.Status.Above50:
                        target = guess.minimumHits == 1 ? "a healthy ally" : "healthy allies";
                        break;
                    case Action.Status.Below50:
                        target = guess.minimumHits == 1 ? "a hurt ally" : "hurt allies";
                        break;
                    case Action.Status.InMelee:
                        target = guess.minimumHits == 1 ? "a close ally" : "close allies";
                        break;
                    case Action.Status.NotInMelee:
                        target = guess.minimumHits == 1 ? "a distant ally" : "distant allies";
                        break;
                }
                break;
            case Action.TargetGroup.Enemies:
                switch (guess.targetConditions[0])
                {
                    case Action.Status.Above50:
                        target = guess.minimumHits == 1 ? "a healthy enemy" : "healthy enemies";
                        break;
                    case Action.Status.Below50:
                        target = guess.minimumHits == 1 ? "a hurt enemy" : "hurt enemies";
                        break;
                    case Action.Status.InMelee:
                        target = guess.minimumHits == 1 ? "a close enemy" : "close enemies";
                        break;
                    case Action.Status.NotInMelee:
                        target = guess.minimumHits == 1 ? "a distant enemy" : "distant enemies";
                        break;
                }
                break;
        }
        if (Book.currentEntry.guess.stats.characterName != "") line.Replace("*", Book.currentEntry.guess.stats.characterName);
        switch (guess.position)
        {
            case Action.Position.Alone:
                line += "is alone, and ";
                break;
            case Action.Position.NearAlly:
                if (guess.nearTargetCount == 1) line += "is near an ally, and ";
                else line += "is near at least * allies, and ";
                line.Replace("*", guess.nearTargetCount.ToString());
                break;
            case Action.Position.NearEnemy:
                if (guess.nearTargetCount == 1) line += "is near an enemy, and ";
                else line += "is near at least * enemies, and ";
                line.Replace("*", guess.nearTargetCount.ToString());
                break;
            case Action.Position.NotNearAlly:
                line += "is not near any allies, and ";
                break;
            case Action.Position.NotNearEnemy:
                line += "is not near any enemiies, and ";
                break;
            case Action.Position.Irrelevant:
                break;
        }
        switch (guess.shape)
        {
            case Action.Shape.Single:
                if (guess.canHitSelf) line += "can target itself";
                else line += "can hit " + target;
                break;
            case Action.Shape.Arc:
                line += "can hit " + (guess.minimumHits == 1 ? "" : "at least " + guess.minimumHits.ToString()) + target + " with a sweep";
                break;
            case Action.Shape.Cone:
                line += "can hit " + (guess.minimumHits == 1 ? "" : "at least " + guess.minimumHits.ToString()) + target + " with a wave";
                break;
            case Action.Shape.Line:
                line += "can hit " + (guess.minimumHits == 1 ? "" : "at least " + guess.minimumHits.ToString()) + target + " with a beam";
                break;
            case Action.Shape.ThreeByThree:
                line += "can hit " + (guess.minimumHits == 1 ? "" : "at least " + guess.minimumHits.ToString()) + target + " with a blast";
                if (guess.canHitSelf) line += "originating from itself";
                break;
        }
        if ((guess.shape == Action.Shape.Single && !guess.canHitSelf) || (guess.shape != Action.Shape.Single))
        {
            switch (guess.targetPriority)
            {
                case Action.TargetPriority.HighestHPCurrent:
                    line += ", prefering the toughest";
                    break;
                case Action.TargetPriority.HighestHPPercent:
                    line += ", prefering the healthiest";
                    break;
                case Action.TargetPriority.LowestHPCurrent:
                    line += ", prefering the weakest";
                    break;
                case Action.TargetPriority.lowestHPPercent:
                    line += ", prefering the most hurt";
                    break;
                case Action.TargetPriority.Closest:
                    line += ", prefering the closest";
                    break;
                case Action.TargetPriority.Farthest:
                    line += ", prefering the farthest";
                    break;
                case Action.TargetPriority.HasSameDebuff:
                    line += ", prefering those already affected";
                    break;
                case Action.TargetPriority.DoesntHaveSameDebuff:
                    line += ", prefering the unaffected";
                    break;
                case Action.TargetPriority.None:
                    break;
            }
        }
        line += ",";
        targetingText.text = line;
    }
    public void OpenOutcome()
    {
        outcome.SetActive(true);
    }
    public void SetOutcome()
    {
        string line = "it will ";
        string target = "";
        if (guess.shape == Action.Shape.Single && guess.canHitSelf) target = "itself";
        else target = "them";
        string damageType = "";
        switch (guess.damageType)
        {
            case Character.DamageTypes.Acid:
                damageType = "acid damage";
                break;
            case Character.DamageTypes.Cold:
                damageType = "cold damage";
                break;
            case Character.DamageTypes.Fire:
                damageType = "fire damage";
                break;
            case Character.DamageTypes.Poison:
                damageType = "poison damage";
                break;
            case Character.DamageTypes.Cutting:
                damageType = "cutting damage";
                break;
            case Character.DamageTypes.Crushing:
                damageType = "crushing damage";
                break;
            case Character.DamageTypes.Piercing:
                damageType = "piercing damage";
                break;
        }
        bool andBuff = false, andDebuff = false;
        switch (guess.actionType)
        {
            case Action.ActionType.Attack:
                line += (guess.isCritical ? "critically " : "") + "hit " + target + " with " + damageType; 
                break;
            case Action.ActionType.AttackDebuff:
                line += (guess.isCritical ? "critically " : "") + "hit " + target + " with " + damageType + " and ";
                andDebuff = true;
                break;
            case Action.ActionType.Healing:
                line += "heal " + target;
                break;
            case Action.ActionType.HealingBuff:
                line += "heal " + target + " and ";
                andBuff = true;
                break;
            case Action.ActionType.Debuff:
                andDebuff = true;
                break;
            case Action.ActionType.Buff:
                andBuff = true;
                break;
        }
        if (andDebuff)
        {
            switch (guess.debuff.debuffName)
            {
                case "Burning":
                    line += "set " + target + " on fire";
                    break;
                case "Acid":
                    line += "cover " + target + " in acid";
                    break;
                case "Bleeding":
                    line += "make " + target + " bleed";
                    break;
                case "Poison":
                    line += "poison " + target;
                    break;
                case "Blind":
                    line += "blind " + target;
                    break;
                case "Taunt":
                    line += "taunt " + target;
                    break;
                case "Root":
                    line += "root " + target;
                    break;
                case "Slow":
                    line += "slow " + target;
                    break;
            }
        }
        if (andBuff)
        {
            switch (guess.buff.buffName)
            {
                case "Armor":
                    break;
                case "Dodge":
                    line += "cause " + target + " to avoid attacks";
                    break;
                case "Regeneration":
                    break;
            }
        }
        line += ".";
        outcomeText.text = line;
    }
}
