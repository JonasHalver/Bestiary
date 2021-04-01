using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HitPointEditor : MonoBehaviour
{
    public TMP_InputField input;
    public Slider slider;
    public HitPointDisplay display, pageDisplay;
    public int value;
    // Start is called before the first frame update
    void Start()
    {

    }
    private void OnEnable()
    {
        value = (int)Book.currentEntry.guess.hitPoints;
        slider.value = value;
        input.text = value.ToString();
        display.ClearHearts();
        display.value = value;
        pageDisplay = Book.currentEntry.page.pageUI.hitpointDisplay;
        HitPointDisplay.editing = true;
        HitPointDisplay.instance = display;
    }
    private void OnDisable()
    {
        HitPointDisplay.editing = false;
        HitPointDisplay.instance = null;
    }

    // Update is called once per frame
    void Update()
    {
        value = Mathf.Clamp(value, 0, 22);
        display.value = value;
    }

    public void Add()
    {
        value++;
        value = Mathf.Clamp(value, 0, 22);

        slider.value = value;
        input.text = value.ToString();
        UpdateStats();
    }
    public void Subtract()
    {
        value--;
        value = Mathf.Clamp(value, 0, 22);

        slider.value = value;
        input.text = value.ToString();
        UpdateStats();
    }

    public void ValueChanged(bool isSlider)
    {
        if (isSlider)
        {
            value = Mathf.RoundToInt(slider.value);
            input.text = value.ToString();
        }
        else
        {
            int.TryParse(input.text, out value);
            slider.value = value;
        }
        UpdateStats();
    }

    public void UpdateStats()
    {
        Book.currentEntry.guess.hitPoints = value;

        Book.UpdateStats();
    }
}
