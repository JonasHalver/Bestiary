using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BookActionDescription : MonoBehaviour
{
    public Transform grid;
    public List<GameObject> buttons = new List<GameObject>();
    public TextMeshProUGUI buttonText;
    
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
        foreach(GameObject b in buttons)
        {
            b.GetComponent<Button>().interactable = true;
        }
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
    }

    public void ResetValues()
    {
        Book.currentEntry.activeDescriptionIndices.Remove(Book.currentEntry.activeAction.guessAction.descriptionIndex);
        Book.currentEntry.activeAction.guessAction.descriptionSet = false;
        Book.currentEntry.activeAction.guessAction.descriptionIndex =-1;
        Book.currentEntry.activeAction.guessAction.actionDescription = null;
        Book.currentEntry.activeAction.originalAction = null;
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
        //BookActionCard.CardUpdate();
        buttonText.text = $"{(currentEntry.guess.characterName != null ? currentEntry.guess.characterName : "The Monster")} {Book.instance.descriptionsList.descriptions[index]}";
        gameObject.SetActive(false);
        //Book.currentEntry.activeAction.CalculateValidity();
    }
    
}
