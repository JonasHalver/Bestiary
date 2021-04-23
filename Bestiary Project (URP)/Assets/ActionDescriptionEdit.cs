using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionDescriptionEdit : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        for (int i = 0; i < Book.instance.descriptionsList.descriptions.Count; i++)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(Book.instance.descriptionsList.descriptions[i]));
        }
    }
    private void Update()
    {
        Book.currentEntry.activeAction.guessAction.descriptionIndex = dropdown.value - 1;
    }
}
