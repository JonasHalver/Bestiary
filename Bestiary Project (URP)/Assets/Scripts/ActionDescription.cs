using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Description List", menuName = "Action Descriptions")]
public class ActionDescription : ScriptableObject
{
    public enum BodyType { Humanoid, Quadroped, Insectoid }
    public List<string> humanoid = new List<string>();
    public List<string> quadroped = new List<string>();
    public List<string> insectoid = new List<string>();

    public List<string> GetList(BodyType bt) 
    {
        switch (bt)
        {
            default: return humanoid;
            case BodyType.Humanoid:
                return humanoid;
            case BodyType.Insectoid:
                return insectoid;
            case BodyType.Quadroped:
                return quadroped;
        }
    }
}
