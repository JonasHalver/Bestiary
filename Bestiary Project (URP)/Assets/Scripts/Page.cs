﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Page : MonoBehaviour
{
    public Entry entry;
    public Transform actionCardHolder;
    public TMP_InputField nameInput;
    public GameObject actionCardPrefab;
    public List<CardRearrangement> actionCards = new List<CardRearrangement>();
    public GameObject addCard;
    public Image icon;
    public StatsEditor editor;
    public PageUI pageUI;
    public GameObject warningPrefab;
    public bool deletionConfirmed = false, cancelConfirmed = false;
    public Book.Chapter chapter;
    public int pageNumber;

    private void Start()
    {
        //if (entry.page == null) entry.page = this;
        //entry.CreateChecks();
        UpdateName();
    }
    private void OnEnable()
    {
        Book.currentEntry = entry;
    }

    public void UpdateName()
    {
        if (Book.currentEntry.isMerc) nameInput.text = Book.currentEntry.origin.characterName;
        else nameInput.text = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : null;
    }

    public void ConnectActions()
    {
        if (entry.origin.characterType == CharacterStats.CharacterTypes.Adventurer)
        {
            for (int i = 0; i < entry.origin.actions.Count; i++)
            {
                NewActionCard();
            }
        }
        //if (actionCards.Count == 0)
        //{
        //    for (int i = 0; i < actionCardHolder.childCount; i++)
        //    {
        //        actionCards.Add(actionCardHolder.GetChild(i).GetComponent<CardRearrangement>());
        //        actionCardHolder.GetChild(i).GetComponent<CardRearrangement>().page = this;
        //    }
        //    for (int i = 0; i < actionCards.Count; i++)
        //    {
        //        actionCards[i].actionCheck = entry.actionChecks[i];
        //        actionCards[i].panelColor = actionCards[i].panelColors[i];
        //        entry.actionChecks[i].panelColor = actionCards[i].panelColor;
        //    }
        //}
    }

    public void MonsterNameChanged()
    {
        entry.guess.characterName = nameInput.text;
    }
    public void ActionPriorityUpdate()
    {
        for (int i = 0; i < actionCards.Count; i++)
        {
            actionCards[i].actionCheck.guessAction.actionPriority = actionCards[i].transform.GetSiblingIndex() + 1;
        }
    }
    public void NewActionCard()
    {
        if (actionCards.Count > 2) addCard.SetActive(false);
        GameObject newCard = Instantiate(actionCardPrefab, actionCardHolder);
        CardRearrangement card = newCard.GetComponent<CardRearrangement>();
        actionCards.Add(card);
        card.page = this;
        card.entry = entry;
        entry.actionCardCount++;
        if (entry.actionChecks.Count >= actionCards.Count) card.actionCheck = entry.actionChecks[actionCards.Count - 1];        
        if (card.panelColors.Count >= actionCards.Count) card.panelColor = card.panelColors[actionCards.Count - 1];
        if (card.actionCheck != null) card.actionCheck.panelColor = card.panelColor;
        addCard.transform.SetAsLastSibling();
        if (GameManager.tutorial && TutorialManager.instance.currentSequence == TutorialManager.TutorialSequence.BestiaryMonster) TutorialManager.instance.ForceContinue(true);
    }
    public void RemoveCard(CardRearrangement card)
    {
        SoundManager.DeletionWarning();
        GameObject newWarning = Instantiate(warningPrefab, transform);
        newWarning.GetComponent<WarningPopup>().page = this;
        entry.actionCardCount--;
        StartCoroutine(WaitingForDeletionConfirmation(card));
    }
    IEnumerator WaitingForDeletionConfirmation(CardRearrangement card)
    {
        while (true)
        {
            if (cancelConfirmed)
            {
                cancelConfirmed = false;
                break;
            }
            if (deletionConfirmed)
            {
                deletionConfirmed = false;
                if (card.entry.activeDescriptionIndices.Contains(card.actionCheck.guessAction.descriptionIndex))
                    card.entry.activeDescriptionIndices.Remove(card.actionCheck.guessAction.descriptionIndex);
                card.actionCheck.guessAction.ResetAction();
                card.actionCheck.originalAction = null;                
                actionCards.Remove(card);
                Destroy(card.gameObject);
                if (actionCards.Count < 4) addCard.SetActive(true);
                break;
            }
            yield return null;
        }
    }
    public void CloseBook()
    {
        GameManager.ChangeState(GameManager.GameState.Normal);
    }

    public void EditingText(bool editing)
    {
        GameManager.textInput = editing;
    }

    public void ChangePage(bool forward)
    {
        Book.instance.ActivePageNumber += forward ? 1 : -1;
        SoundManager.PageChange(forward);
        Book.instance.PageChange();
    }
}
