using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InitiativeCard : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public Character actor;
    public Image background;
    public Image highlight;
    public Image icon;
    public TextMeshProUGUI description;
    public Color bgMonster, bgMerc, bgHighlight;
    public bool isMerc;
    public int initiative = 0;
    private void OnEnable()
    {
        CombatManager.CurrentTurn += HighlightCard;
    }
    private void OnDisable()
    {
        CombatManager.CurrentTurn -= HighlightCard;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        CharacterSheet.instance.ShowEntry(actor);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
        {
            actor.HighlightAction();
        }
        CharacterSheet.ShowSheet(actor);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
            CombatGrid.StopHighlight();
    }

    private void HighlightCard(Character character)
    {
        if (character == actor)
        {
            highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 1);
        }
        else
        {
            highlight.color = new Color(highlight.color.r, highlight.color.g, highlight.color.b, 0);
        }
    }

    private void UpdateCard()
    {        
        isMerc = actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer;
        if (actor.alive && actor.currentAction.highlighted) background.color = bgHighlight;
        else background.color = isMerc ? bgMerc : bgMonster;
        icon.sprite = actor.stats.characterIcon;
        icon.color = actor.stats.characterColor;
        string d = "";
        string cname = "";
        Entry e = null;
        if (isMerc)
        {
            initiative = actor.stats.speed;
            cname = actor.stats.characterName;
            if (actor.alive && actor.Conditions.ContainsKey(Action.Condition.Stun)) d = $"{cname} is stunned.";
            else if (actor.alive && !actor.currentAction.action.isPass)
                d = $"{cname} will use {actor.currentAction.action.actionName}";
            else if (actor.alive && actor.currentAction.action.isPass) d = $"{cname} will pass.";
        }
        else
        {
            for (int i = 0; i < Book.monsterEntries.Count; i++)
            {
                
                if (Book.monsterEntries[i].origin.characterCode.Equals(actor.stats.characterCode))
                {
                    e = Book.monsterEntries[i];
                    cname = e.guess.characterName != null ? e.guess.characterName : "The monster";
                    if (cname.Length == 0) cname = "The monster";
                    if (!actor.alive)
                        break;

                    if (actor.currentAction.action.isPass)
                    {
                        d = $"{cname} will pass.";
                        break;
                    }
                    if (actor.Conditions.ContainsKey(Action.Condition.Stun))
                    {
                        d = $"{cname} is stunned.";
                    }

                    initiative = e.guess.speed - ((actor.Conditions.ContainsKey(Action.Condition.SlowMonster) || actor.Conditions.ContainsKey(Action.Condition.SlowMerc)) ? 2 : 0);
                    if (actor.alive)
                    {
                        bool flag = false;
                        for (int j = 0; j < e.actionChecks.Count; j++)
                        {
                            if (e.actionChecks[j].originalAction != null)
                            {
                                if (actor.currentAction.action.actionCode.Equals(e.actionChecks[j].originalAction.actionCode))
                                {
                                    d = cname + " will use " + e.actionChecks[j].guessAction.actionName;
                                    flag = true;
                                    break;
                                }
                                else
                                {
                                    d = cname + " " + Book.instance.descriptionsList.descriptions[actor.currentAction.action.descriptionIndex];

                                }
                                if (flag) break;
                            }                            
                            else
                            {
                                d = cname + " " + Book.instance.descriptionsList.descriptions[actor.currentAction.action.descriptionIndex];
                            }
                        }
                    }

                }
            }            
        }
        if (!actor.alive)
        {
            d = $"{cname} is dead.";
        }
        description.text = d;
    }
}
