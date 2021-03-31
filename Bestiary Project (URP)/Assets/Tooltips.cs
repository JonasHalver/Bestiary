using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Tooltip Collection", menuName = "Tooltip Collection")]
public class Tooltips : ScriptableObject
{
    public List<string> damageTypes = new List<string>();
    public List<string> shapes = new List<string>();

    public string DamageTypeString(Character.DamageTypes type)
    {
        return damageTypes[(int)type];   
    }
    public string ShapeString(int shapeIndex)
    {
        return shapes[shapeIndex];
    }
}
