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
    private List<TextMeshProUGUI> actionTexts = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> titleTexts = new List<TextMeshProUGUI>();

    public GameObject showEntryButton;
    public Entry currentEntry;

    private void Awake()
    {
        instance = this;
        actionTexts.Add(action1); actionTexts.Add(action2); actionTexts.Add(action3); actionTexts.Add(action4);
        titleTexts.Add(title1); titleTexts.Add(title2); titleTexts.Add(title3); titleTexts.Add(title4); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void ShowSheet(Character character)
    {
        instance.currentCharacter = character;
        for (int i = 0; i < Book.monsterEntries.Count; i++)
        {
            if (Book.monsterEntries[i].origin.characterCode.Equals(character.stats.characterCode))
            {
                instance.currentEntry = Book.monsterEntries[i];
                break;
            }
        }
        CharacterStats stats = character.stats.characterType == CharacterStats.CharacterTypes.Adventurer ? character.stats : instance.currentEntry.guess;
        instance.characterName.text = stats.characterName != null ? stats.characterName : "Unknown Monster";
        if (instance.characterName.text.Length == 0) instance.characterName.text = "Unknown Monster";
        instance.characterActions.Clear();

        for (int i = 0; i <= stats.actions.Count; i++)
        {
            foreach(Action a in stats.actions)
            {
                if (a.actionPriority == i) instance.characterActions.Add(a);
            }
        }
        foreach(TextMeshProUGUI text in instance.actionTexts)
        {
            text.text = "";
        }
        foreach (TextMeshProUGUI text in instance.titleTexts)
        {
            text.text = "";
        }

        for (int i = 0; i < instance.characterActions.Count; i++)
        {
            Action a = instance.characterActions[i];
            if (a != null)
            {
                instance.actionTexts[i].text =  a.description;
                instance.titleTexts[i].text = (i + 1).ToString() + ". " + a.actionName;
            }
        }
        instance.showEntryButton.SetActive(character.stats.characterType == CharacterStats.CharacterTypes.NPC);
        instance.sheet.SetActive(true);
    }

    public void ShowEntry(Character character)
    {
        Book.openOnMerc = character.stats.characterType == CharacterStats.CharacterTypes.Adventurer;

        Book.instance.pageNumber = character == null ? currentCharacter.stats.pageNumber : character.stats.pageNumber;
        GameManager.ChangeState(GameManager.GameState.Journal);
        HideSheet();
    }

    public void HideSheet()
    {
        currentCharacter = null;
        sheet.SetActive(false);
    }
}
