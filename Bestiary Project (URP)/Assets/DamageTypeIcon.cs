using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DamageTypeIcon : MonoBehaviour
{
    public bool isNone = false;
    public bool isEditable = false;
    public Character.DamageTypes damageType;
    public bool isResistance;
    public Color resistanceColor, weaknessColor;
    public TextMeshProUGUI text;
    public Image icon, background;
    public GameObject removal, manager;

    // Update is called once per frame
    void Start()
    {
        if (!isNone)
        {
            icon.sprite = GameManager.Icon(damageType);
            background.color = isResistance ? resistanceColor : weaknessColor;
            text.text = damageType.ToString();
        }
    }
    public void DisplayRemove(bool display)
    {
        if (!isEditable) return;
        removal.SetActive(display);
    }
    public void Remove()
    {
        manager.SendMessage("RemoveDamageTypeIcon", this);
    }
}
