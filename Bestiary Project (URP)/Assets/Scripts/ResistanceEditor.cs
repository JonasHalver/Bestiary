using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ResistanceEditor : MonoBehaviour
{
    public bool isResistance;
    public Transform grid;
    public List<Character.DamageTypes> currentTypes = new List<Character.DamageTypes>();
    public GameObject selectionPrefab;
    public List<Selection> selections = new List<Selection>();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject newSelection = Instantiate(selectionPrefab, grid);
            Selection s = newSelection.GetComponent<Selection>();
            s.icon.sprite = GameManager.instance.currentIconCollection.GetIcon((Character.DamageTypes)i).icon;
            s.icon.color = GameManager.instance.currentIconCollection.GetIcon((Character.DamageTypes)i).iconColor;
            s.text.text = ((Character.DamageTypes)i).ToString();
            selections.Add(s);
        }
    }

    private void OnEnable()
    {
        Selection.Selected += UpdateStats;
        CharacterStats stats = Book.currentEntry.guess;
        currentTypes.Clear();
        for (int i = 0; i < (isResistance ? stats.resistances.Count : stats.weaknesses.Count); i++)
        {
            currentTypes.Add(isResistance ? stats.resistances[i] : stats.weaknesses[i]);
        }
        for (int i = 0; i < currentTypes.Count; i++)
        {
            selections[(int)currentTypes[i]].selected = true;
        }
    }
    private void OnDisable()
    {
        Selection.Selected -= UpdateStats;
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateStats()
    {
        currentTypes.Clear();
        for (int i = 0; i < selections.Count; i++)
        {
            if (selections[i].selected)
            {
                currentTypes.Add((Character.DamageTypes)i);

            }
        }
        if (isResistance)
        {
            Book.currentEntry.guess.resistances.Clear();
            for (int i = 0; i < currentTypes.Count; i++)
            {
                Book.currentEntry.guess.resistances.Add(currentTypes[i]);
            }
        }
        else
        {
            Book.currentEntry.guess.weaknesses.Clear();
            for (int i = 0; i < currentTypes.Count; i++)
            {
                Book.currentEntry.guess.weaknesses.Add(currentTypes[i]);
            }
        }

        Book.UpdateStats();
    }
}
