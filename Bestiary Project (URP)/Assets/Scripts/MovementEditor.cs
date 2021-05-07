using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MovementEditor : MonoBehaviour
{
    public TMP_InputField input;
    public int value = 1;
    public Color valid, invalid;
    public List<Image> tiles = new List<Image>();
    public Image characterIcon;
    public Transform grid;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 1; i < grid.childCount; i++)
        {
            tiles.Add(grid.GetChild(i).GetComponent<Image>());
        }
    }

    private void OnEnable()
    {
        value = Book.currentEntry.guess.movement;
        input.text = value.ToString();
        characterIcon.sprite = Book.currentEntry.origin.characterIcon;
        characterIcon.color = Book.currentEntry.origin.characterColor;
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value, 1, 4);
        for (int i = 0; i < tiles.Count; i++)
        {
            if (i < value)
                tiles[i].color = valid;
            else
                tiles[i].color = invalid;                    
        }
    }

    public void ClickedTile(int index)
    {
        value = index;
        input.text = value.ToString();
        UpdateStats();
    }

    public void ValueChanged()
    {
        int.TryParse(input.text, out value);
        UpdateStats();
    }

    public void Add()
    {
        value++;
        value = Mathf.Clamp(value, 1, 4);

        input.text = value.ToString();
        UpdateStats();
    }
    public void Subtract()
    {
        value--;
        value = Mathf.Clamp(value, 1, 4);

        input.text = value.ToString();
        UpdateStats();
    }

    public void UpdateStats()
    {        
        Book.currentEntry.guess.movement = value;
        Book.UpdateStats();
    }
}
