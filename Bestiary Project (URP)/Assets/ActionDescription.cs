using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Description List", menuName = "Action Descriptions")]
public class ActionDescription : ScriptableObject
{
    public List<string> descriptions = new List<string>();
}
