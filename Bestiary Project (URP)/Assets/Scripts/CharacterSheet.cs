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

        for (int i = 0; i < instance.cards.Count; i++)
        {
            for (int j = 0; j < instance.currentEntry.actionChecks.Count; j++)
            {
                if (instance.currentEntry.actionChecks[j].guessAction.actionPriority == i+1)
                {
                    // instance.cards[i].ChangeInfo(instance.currentEntry.actionChecks[j]);
                }                
            }
        }
        instance.sheet.SetActive(true);
    }

    public void ShowEntry(Character character)
    {
        Book.openOnMerc = character.stats.characterType == CharacterStats.CharacterTypes.Adventurer;

        Book.instance.pageNumber = character == null ? currentCharacter.stats.pageNumber : character.stats.pageNumber;
        GameManager.ChangeState(GameManager.GameState.Bestiary);
        HideSheet();
    }
    public void ShowEntry(Entry entry)
    {
        Book.openOnMerc = entry.isMerc;

        Book.instance.pageNumber = entry == null ? currentCharacter.stats.pageNumber : entry.origin.pageNumber;
        GameManager.ChangeState(GameManager.GameState.Bestiary);
        HideSheet();
    }

    public void HideSheet()
    {
        currentCharacter = null;
        sheet.SetActive(false);
    }
}
