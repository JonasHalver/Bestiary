using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryManager : MonoBehaviour
{
    public GameObject stats, buffs, combat, shapes;
    public Button statsButton, buffsButton, combatButton, shapesButton;

    private void Start()
    {
        Stats();
    }

    public void Stats()
    {
        stats.SetActive(true);
        buffs.SetActive(false);
        combat.SetActive(false);
        shapes.SetActive(false);
        statsButton.interactable = false;
        buffsButton.interactable = true;
        combatButton.interactable = true;
        shapesButton.interactable = true;
    }
    public void Buffs()
    {
        stats.SetActive(false);
        buffs.SetActive(true);
        combat.SetActive(false);
        shapes.SetActive(false);
        statsButton.interactable = true;
        buffsButton.interactable = false;
        combatButton.interactable = true;
        shapesButton.interactable = true;
    }
    public void Combat()
    {
        stats.SetActive(false);
        buffs.SetActive(false);
        combat.SetActive(true);
        shapes.SetActive(false);
        statsButton.interactable = true;
        buffsButton.interactable = true;
        combatButton.interactable = false;
        shapesButton.interactable = true;
    }
    public void Shapes()
    {
        stats.SetActive(false);
        buffs.SetActive(false);
        combat.SetActive(false);
        shapes.SetActive(true);
        statsButton.interactable = true;
        buffsButton.interactable = true;
        combatButton.interactable = true;
        shapesButton.interactable = false;
    }
}
