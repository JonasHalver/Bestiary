using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CharacterSheet : MonoBehaviour
{
    public static CharacterSheet instance;
    public GameObject sheet;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI action1, action2, action3, action4;
    public TextMeshProUGUI title1, title2, title3, title4;
    public Character currentCharacter;

    private List<Action> characterActions = new List<Action>();

    public List<CharacterSheetCard> cards = new List<CharacterSheetCard>();

    public GameObject showEntryButton;
    public Entry currentEntry;
    public GameObject noActions;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ShowSheet(Character character)
    {
        instance.currentCharacter = character;
        instance.currentEntry = character.entry;

        CharacterStats stats = instance.currentEntry.isMerc ? character.stats : instance.currentEntry.guess;
        instance.characterName.text = stats.characterName != null ? stats.characterName : "Unknown Monster";
        if (instance.characterName.text.Length == 0) instance.characterName.text = "Unknown Monster";
        instance.characterActions.Clear();
        instance.sheet.SetActive(true);
        if (instance.currentEntry.actionCardCount == 0)
        {
            for (int i = 0; i < instance.cards.Count; i++)
            {
                instance.cards[i].gameObject.SetActive(false);
            }
            instance.noActions.SetActive(true);
        }
        else
        {
            instance.noActions.SetActive(false);
            for (int i = 0; i < instance.cards.Count; i++)
            {
                if (i < instance.currentEntry.actionCardCount) instance.cards[i].gameObject.SetActive(true);
                else
                {
                    instance.cards[i].gameObject.SetActive(false);
                    continue;
                }
                for (int j = 0; j < instance.currentEntry.actionChecks.Count; j++)
                {
                    if (instance.currentEntry.actionChecks[j].guessAction.actionPriority == i + 1)
                    {
                        instance.cards[i].ChangeInfo(instance.currentEntry.actionChecks[j]);
                    }
                }
            }
        }
    }

    public void ShowEntry(Character character)
    {
        Book.instance.currentChapter = currentEntry.isMerc ? Book.Chapter.Mercenaries : Book.Chapter.Monsters;
        Book.instance.ActivePageNumber = currentEntry.origin.pageNumber;
        GameManager.ChangeState(GameManager.GameState.Bestiary);
        //HideSheet();
    }
    public void ShowEntry(Entry entry)
    {
        Book.instance.currentChapter = entry.isMerc ? Book.Chapter.Mercenaries : Book.Chapter.Monsters;
        Book.instance.ActivePageNumber = entry.origin.pageNumber;

        GameManager.ChangeState(GameManager.GameState.Bestiary);
        //HideSheet();
    }

    public void HideSheet()
    {
        currentCharacter = null;
        sheet.SetActive(false);
    }
}
