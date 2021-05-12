using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterImage : MonoBehaviour
{
    public Page page;
    private Image img;
    private Entry entry;
    private Sprite sprite;

    private void Start()
    {
        img = GetComponent<Image>();
        
    }
    private void Update()
    {
        entry = page.entry;
        sprite = entry.origin.characterModel.GetComponent<Image>().sprite;
        img.sprite = sprite;
        img.color = entry.origin.characterColor;
    }
}
