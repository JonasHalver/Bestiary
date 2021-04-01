using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Page : MonoBehaviour
{
    public Entry entry;
    public Transform actionCardHolder;
    public TMP_InputField nameInput;
    public List<CardRearrangement> actionCards = new List<CardRearrangement>();
    public Image icon;
    public StatsEditor editor;
    public PageUI pageUI;


    private void Start()
    {
        //if (entry.page == null) entry.page = this;
        //entry.CreateChecks();
    }


    public void UpdateName()
    {
        if (Book.currentEntry.isMerc) nameInput.text = Book.currentEntry.origin.characterName;
        else nameInput.text = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : null;
    }

    public void ConnectActions()
    {
        if (actionCards.Count == 0)
        {
            for (int i = 0; i < actionCardHolder.childCount; i++)
            {
                actionCards.Add(actionCardHolder.GetChild(i).GetComponent<CardRearrangement>());
                actionCardHolder.GetChild(i).GetComponent<CardRearrangement>().page = this;
            }
            for (int i = 0; i < actionCards.Count; i++)
            {
                actionCards[i].actionCheck = entry.actionChecks[i];
                actionCards[i].panelColor = actionCards[i].panelColors[i];
                entry.actionChecks[i].panelColor = actionCards[i].panelColor;
            }
        }
    }

    public void MonsterNameChanged()
    {
        print(entry.guess.characterName);
        print(nameInput.text);
        entry.guess.characterName = nameInput.text;
    }
    public void ActionPriorityUpdate()
    {
        for (int i = 0; i < actionCards.Count; i++)
        {
            actionCards[i].actionCheck.guessAction.actionPriority = actionCards[i].transform.GetSiblingIndex() + 1;

        }
    }

    public void CloseBook()
    {
        GameManager.instance.OpenJournal();
    }

    public void EditingText(bool editing)
    {
        GameManager.textInput = editing;
    }

    public void ChangePage(bool forward)
    {
        Book.instance.pageNumber += forward ? 1 : -1;
        Book.instance.PageChange();
    }
}
