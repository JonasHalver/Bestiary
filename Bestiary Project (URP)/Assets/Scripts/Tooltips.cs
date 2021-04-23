using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tooltip Collection", menuName = "Tooltip Collection")]
public class Tooltips : ScriptableObject
{
    public enum TooltipType { Condition, DamageType, Shape, Custom, Priority }

    public List<string> damageTypes = new List<string>();
    public List<string> shapes = new List<string>();
    public List<string> conditions = new List<string>();
    public List<string> priorities = new List<string>();

    public string GetString(Character.DamageTypes type)
    {
        return damageTypes[(int)type];   
    }
    public string GetString(Action.Shape shape)
    {
        return shapes[(int)shape];
    }
    public string GetString(Action.Condition condition)
    {
        return conditions[(int)condition];
    }
    public string GetString(Action.TargetPriority priority)
    {
        return priorities[(int)priority];
    }
}
