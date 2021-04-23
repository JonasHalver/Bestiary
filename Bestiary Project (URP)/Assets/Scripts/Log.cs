using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "Log Elements", menuName = "Log Elements")]
public class Log : ScriptableObject
{
    public List<Entry> contextEntries = new List<Entry>();
    public List<Entry> outputEntries = new List<Entry>();
    public List<Entry> shapeEntries = new List<Entry>();

    // General Context
    public string GetString(Action.Context context)
    {
        for (int i = 0; i < contextEntries.Count; i++)
        {
            if (contextEntries[i].context == context) return contextEntries[i].text;
        }
        return null;
    }
    // Context with specific Damage Type
    public string GetString(Action.Context context, Character.DamageTypes dt)
    {
        string s = "";
        for (int i = 0; i < contextEntries.Count; i++)
        {
            if (contextEntries[i].context == context) s += contextEntries[i].text;
        }
        s = s.Replace("*", dt.ToString());
        return s;
    }
    // Context with specific condition
    public string GetString(Action.Context context, Action.Condition c)
    {
        string s = "";
        string cs = "";
        switch (c)
        {
            default:
                cs = c.ToString();
                break;
            case Action.Condition.DisorientMerc:
            case Action.Condition.DisorientMonster:
                cs = "Disorient";
                break;
            case Action.Condition.FearMerc:
            case Action.Condition.FearMonster:
                cs = "Fear";
                break;
            case Action.Condition.RootMerc:
            case Action.Condition.RootMonster:
                cs = "Root";
                break;
            case Action.Condition.SlowMonster:
            case Action.Condition.SlowMerc:
                cs = "Slow";
                break;
            case Action.Condition.TauntMerc:
            case Action.Condition.TauntMonster:
                cs = "Taunt";
                break;
        }
        for (int i = 0; i < contextEntries.Count; i++)
        {
            if (contextEntries[i].context == context) s+= contextEntries[i].text;
        }
        s = s.Replace("*", cs);
        return s;
    }
    // Output - Damage
    public string GetString(Action.Output output, Character.DamageTypes dt, bool crit)
    {
        string s = "";
        string c = crit ? "critical " : "";
        for (int i = 0; i < outputEntries.Count; i++)
        {
            if (outputEntries[i].output == output) s += outputEntries[i].text;
        }
        s = s.Replace("^", c);
        s = s.Replace("*", dt.ToString());
        return s;
    }
    // Output - Condition
    public string GetString(Action.Output output, Action.Condition c, int value)
    {
        string s = "";
        string v = value.ToString();
        string cs = "";
        switch (c)
        {
            default:
                cs = c.ToString();
                break;
            case Action.Condition.DisorientMerc:
            case Action.Condition.DisorientMonster:
                cs = "Disorient";
                break;
            case Action.Condition.FearMerc:
            case Action.Condition.FearMonster:
                cs = "Fear";
                break;
            case Action.Condition.RootMerc:
            case Action.Condition.RootMonster:
                cs = "Root";
                break;
            case Action.Condition.SlowMonster:
            case Action.Condition.SlowMerc:
                cs = "Slow";
                break;
            case Action.Condition.TauntMerc:
            case Action.Condition.TauntMonster:
                cs = "Taunt";
                break;
        }

        for (int i = 0; i < outputEntries.Count; i++)
        {
            if (outputEntries[i].output == output) s += outputEntries[i].text;
        }
        s = s.Replace("^", v);
        s = s.Replace("*", cs);
        return s;
    }
    // Output - Healing
    public string GetString(Action.Output output, int value)
    {
        string s = "";
        string v = value.ToString();
        for (int i = 0; i < outputEntries.Count; i++)
        {
            if (outputEntries[i].output == output) s += outputEntries[i].text;
        }
        s = s.Replace("*", v);
        return s;
    }
    // Output - Movement
    public string GetString(Action.Output output, int value, bool towards)
    {
        string s = "";
        string t = towards ? "closer" : "further away";
        string v = value.ToString();
        string p = value > 1 ? "s" : "";
        for (int i = 0; i < outputEntries.Count; i++)
        {
            if (outputEntries[i].output == output) s += outputEntries[i].text;
        }
        s = s.Replace("^", t);
        s = s.Replace("*", v);
        s = s.Replace("%", p);
        return s;
    }
    // Shape, targetgroup and targeting
    public string GetString(Action.Shape shape, Action.TargetGroup targetGroup, Action.Targeting targeting)
    {
        string s = "";
        string t = targeting == Action.Targeting.Ground ? " standing" : "";
        string tg = shape != Action.Shape.Self ? targetGroup.ToString().ToLower() : "";
        if (shape == Action.Shape.Melee || shape == Action.Shape.Ranged)
        {
            if (targetGroup == Action.TargetGroup.Enemies) tg = "an enemy";
            else if (targetGroup == Action.TargetGroup.Allies) tg = "an ally";
            else tg = "a character";
        }
        if (shape == Action.Shape.All && targetGroup == Action.TargetGroup.All) tg = "characters";
        for (int i = 0; i < shapeEntries.Count; i++)
        {
            if (shapeEntries[i].shape== shape) s += shapeEntries[i].text;
        }
        s = s.Replace("^", t);
        s = s.Replace("*", tg);
        return s;
    }


    [Serializable]
    public class Entry
    {
        public string text;
        public Action.Context context;
        public Action.Output output;
        public Action.Shape shape;
    }
}

