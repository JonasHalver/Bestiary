using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ShapeEdit : MonoBehaviour
{
    public static ShapeEdit instance;
    public ActionNode node;
    public Image shapeIcon1, shapeIcon2;
    public TextMeshProUGUI title, description;
    private bool changesMade;
    public void ShowEditing()
    {
        instance = this;
        title.text = node.nodeName;
        description.text = node.nodeDescription;
        Icons collection = GameManager.instance.currentIconCollection;
        switch (node.actionShape)
        {
            case Action.Shape.Melee:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Melee).icon;
                shapeIcon2.sprite = collection.meleeAlternate;
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Melee;
                shapeIcon2.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Melee;
                break;
            case Action.Shape.Ranged:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Ranged).icon;
                shapeIcon2.gameObject.SetActive(false);
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Ranged;
                break;
            case Action.Shape.Self:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Self).icon;
                shapeIcon2.gameObject.SetActive(false);
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Self;
                break;
            case Action.Shape.Arc:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Arc).icon;
                shapeIcon2.sprite = collection.arcAlternate;
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Arc;
                shapeIcon2.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Arc;
                break;
            case Action.Shape.Cone:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Cone).icon;
                shapeIcon2.sprite = collection.coneAlternate;
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Cone;
                shapeIcon2.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Cone;
                break;
            case Action.Shape.Line:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Line).icon;
                shapeIcon2.sprite = collection.lineAlternate;
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Line;
                shapeIcon2.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Line;
                break;
            case Action.Shape.Pulse:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Pulse).icon;
                shapeIcon2.gameObject.SetActive(false);
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Pulse;
                break;
            case Action.Shape.Area:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.Area).icon;
                shapeIcon2.gameObject.SetActive(false);
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.Area;
                break;
            case Action.Shape.All:
                shapeIcon1.sprite = collection.GetIcon(Action.Shape.All).icon;
                shapeIcon2.gameObject.SetActive(false);
                shapeIcon1.GetComponent<SimpleTooltipSpawner>().shape = Action.Shape.All;
                break;
        }
    }
    public void CloseWindow()
    {
        Destroy(gameObject);
    }
}
