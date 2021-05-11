using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MovementEditor : MonoBehaviour
{
    public TMP_InputField input;
    public int value = 1;
    public Image g1, g2, g3, g4;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnEnable()
    {
        value = Book.currentEntry.guess.movement;
        input.text = value.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value, 1, 4);
        ShowValue();
    }

    private void ShowValue()
    {
        switch (value)
        {
            case 1:
                g1.enabled = true;
                g2.enabled = false;
                g3.enabled = false;
                g4.enabled = false;
                break;
            case 2:
                g1.enabled = false;
                g2.enabled = true;
                g3.enabled = false;
                g4.enabled = false;
                break;
            case 3:
                g1.enabled = false;
                g2.enabled = false;
                g3.enabled = true;
                g4.enabled = false;
                break;
            case 4:
                g1.enabled = false;
                g2.enabled = false;
                g3.enabled = false;
                g4.enabled = true;
                break;
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
