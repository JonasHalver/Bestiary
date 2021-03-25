﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookActionDescription : MonoBehaviour
{
    public TextMeshProUGUI title;
    public Transform grid;
    public List<GameObject> buttons = new List<GameObject>();
    private void Awake()
    {
        if (buttons.Count == 0)
        {
            for (int i = 0; i < grid.childCount; i++)
            {
                buttons.Add(grid.GetChild(i).gameObject);
            }
        }
        GenerateList();
    }
    private void OnEnable()
    {
    }

    public void GenerateList()
    {
        List<string> descriptions = Book.instance.descriptionsList.descriptions;

        for (int i = 0; i < buttons.Count; i++)
        {
            TextMeshProUGUI text = buttons[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = descriptions[i];
        }
    }

    private void Update()
    {
        for (int i = 0; i < Book.currentEntry.activeDescriptionIndices.Count; i++)
        {
            int index = Book.currentEntry.activeDescriptionIndices[i];
            if (index > -1) buttons[index].GetComponent<Button>().interactable = false;
        }
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.characterName != null ? Book.currentEntry.guess.characterName : "the monster";

        titleText = titleText.Replace("*", replacement);
        title.text = titleText;

    }

    public void NewSelection(int index)
    {
        Entry currentEntry = Book.currentEntry;
        if (currentEntry.activeDescriptionIndices.Contains(index)) return;
        foreach(GameObject button in buttons)
        {
            button.GetComponent<Button>().interactable = true;
        }
        if (currentEntry.activeAction.guessAction.descriptionIndex == -1)
        {
            currentEntry.activeDescriptionIndices.Add(index);
        }
        else
        {
            currentEntry.activeDescriptionIndices.Remove(currentEntry.activeAction.guessAction.descriptionIndex);
            currentEntry.activeDescriptionIndices.Add(index);
        }
        currentEntry.activeAction.guessAction.descriptionSet = true;
        currentEntry.activeAction.guessAction.descriptionIndex = index;
        currentEntry.activeAction.guessAction.actionDescription = Book.instance.descriptionsList.descriptions[index];
        BookActionCard.CardUpdate();

        gameObject.SetActive(false);
        Book.currentEntry.activeAction.CalculateValidity();

    }
}
