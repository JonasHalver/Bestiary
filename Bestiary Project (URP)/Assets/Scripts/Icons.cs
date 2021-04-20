using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Icon Collection", menuName = "Icon Collection")]
public class Icons : ScriptableObject
{
    public List<Properties> damageTypes = new List<Properties>();
    public List<Properties> conditions = new List<Properties>();
    public List<Properties> shapes = new List<Properties>();

    public Properties GetIcon(Action.Condition condition)
    {
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].condition == condition) return conditions[i];
        }
        return null;
    }
    public Properties GetIcon(Action.Shape shape)
    {
        for (int i = 0; i < shapes.Count; i++)
        {
            if (shapes[i].shape == shape) return shapes[i];
        }
        return null;
    }
    public Properties GetIcon(Character.DamageTypes damageType)
    {
        for (int i = 0; i < damageTypes.Count; i++)
        {
            if (damageTypes[i].damageType == damageType) return damageTypes[i];
        }
        return null;
    }

    [Serializable]
    public class Properties
    {
        public Sprite icon;
        public Color iconColor = Color.white;
        public Action.Condition condition = Action.Condition.None;
        public Character.DamageTypes damageType = Character.DamageTypes.None;
        public Action.Shape shape;
    }
}

