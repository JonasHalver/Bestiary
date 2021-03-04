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

    private List<Action> characterActions = new List<Action>();
    private List<TextMeshProUGUI> actionTexts = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> titleTexts = new List<TextMeshProUGUI>();


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
        instance.characterName.text = character.stats.characterName;
        instance.characterActions.Clear();
        for (int i = 1; i <= character.stats.actions.Count; i++)
        {
            foreach(Action a in character.stats.actions)
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
            if (instance.characterActions[i] != null)
            {
                instance.actionTexts[i].text = instance.characterActions[i].description;
                instance.titleTexts[i].text = instance.characterActions[i].actionName;
            }
        }
        instance.sheet.SetActive(true);
    }

    public void HideSheet()
    {
        sheet.SetActive(false);
    }
}
