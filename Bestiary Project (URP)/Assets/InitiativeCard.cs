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
    public Image icon;
    public TextMeshProUGUI description;
    public Color bgMonster, bgMerc, bgHighlight;
    public bool isMerc;
    public int initiative = 0;

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

    private void UpdateCard()
    {
        isMerc = actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer;
        if (actor.currentAction.highlighted) background.color = bgHighlight;
        else background.color = isMerc ? bgMerc : bgMonster;
        icon.sprite = actor.stats.characterIcon;
        icon.color = actor.stats.characterIconColor;

        string d = "";
        if (isMerc)
        {
            initiative = actor.stats.speed;
            d = actor.stats.characterName + " will use " + actor.currentAction.action.actionName;
        }
        else
        {
            bool flag = false;
            for (int i = 0; i < Book.monsterEntries.Count; i++)
            {
                if (Book.monsterEntries[i].origin.characterCode.Equals(actor.stats.characterCode))
                {
                    Entry e = Book.monsterEntries[i];
                    initiative = e.guess.speed - (actor.conditions.Contains(Debuff.ControlType.Slow) ? 2 : 0);
                    for (int j = 0; j < e.actionChecks.Count; j++)
                    {
                        if (e.actionChecks[j].originalAction != null)
                        {
                            if (actor.currentAction.action.actionCode.Equals(e.actionChecks[j].originalAction.actionCode))
                            {
                                d = (e.guess.characterName != null ? e.guess.characterName : "The monster") + " will use " + e.actionChecks[j].guessAction.actionName;
                                flag = true;
                                break;
                            }
                            else
                            {
                                d = (e.guess.characterName != null ? e.guess.characterName : "The monster") + " " + Book.instance.descriptionsList.descriptions[actor.currentAction.action.descriptionIndex];

                            }
                        }
                        else
                        {
                            d = (e.guess.characterName != null ? e.guess.characterName : "The monster") + " " + Book.instance.descriptionsList.descriptions[actor.currentAction.action.descriptionIndex];

                        }
                    }
                    if (flag) break;
                }
            }
        }
        description.text = d;
    }
}
