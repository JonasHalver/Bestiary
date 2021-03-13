using System.Collections;
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
        string titleText = title.text;
        string replacement = Book.currentEntry.guess.stats.characterName != "" ? Book.currentEntry.guess.stats.characterName : "the monster";
        titleText.Replace("*", replacement);
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
        if (Book.currentEntry.activeAction.guessAction.descriptionIndex > -1)
            buttons[Book.currentEntry.activeAction.guessAction.descriptionIndex].GetComponent<Button>().interactable = false;
    }

    public void NewSelection(int index)
    {
        foreach(GameObject button in buttons)
        {
            button.GetComponent<Button>().interactable = true;
        }
        Book.currentEntry.activeAction.guessAction.descriptionIndex = index;
        Book.currentEntry.activeAction.guessAction.actionDescription = Book.instance.descriptionsList.descriptions[index];
    }
}
