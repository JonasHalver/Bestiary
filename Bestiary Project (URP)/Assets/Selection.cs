using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Selection : MonoBehaviour
{
    public Image background;
    public Image icon;
    public TextMeshProUGUI text;
    public Color selectedColor;
    public bool selected;
    public static event System.Action Selected;

    private void Update()
    {
        background.color = selected ? selectedColor : Color.white;

    }
    public void Clicked()
    {
        selected = !selected;
        Selected.Invoke();
    }
}
