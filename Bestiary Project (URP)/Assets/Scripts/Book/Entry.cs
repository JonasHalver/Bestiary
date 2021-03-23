using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public CharacterStats origin;
    public CharacterStats guess;
    public Page page;

    public bool isMerc = false;

    public ActionCheck activeAction;

    public enum StatEntries { HitPoints, Movement, Speed, Resistances, Weaknesses }

    public Dictionary<StatEntries, bool> statChecks = new Dictionary<StatEntries, bool>();
    public List<int> activeDescriptionIndices = new List<int>();

    public List<ActionCheck> actionChecks = new List<ActionCheck>();

    

    public Entry (CharacterStats _origin)
    {
        origin = _origin;
        CharacterStats character = new CharacterStats();
        guess = character;
    }

    private void Awake()
    {
        
    }
    public void CreateChecks()
    {
        string defaultDescription = "When the monster..." + System.Environment.NewLine + "if..." + System.Environment.NewLine + "then...";
        statChecks.Add(StatEntries.HitPoints, false); statChecks.Add(StatEntries.Speed, false); statChecks.Add(StatEntries.Movement, false);
        statChecks.Add(StatEntries.Resistances, false); statChecks.Add(StatEntries.Weaknesses, false);
        guess.characterType = CharacterStats.CharacterTypes.NPC;
        guess.speed = 5;
        guess.movement = 2;
        guess.hitPoints = 1;
        for (int i = 0; i < 4; i++)
        {
            Action newAction = ScriptableObject.CreateInstance<Action>();
            actionChecks.Add(new ActionCheck(origin, newAction));
            guess.actions.Add(newAction);
        }
        for (int i = 0; i < actionChecks.Count; i++)
        {
            actionChecks[i].guessAction.actionPriority = i;
            actionChecks[i].guessAction.actionName = "Unknown Action";
            actionChecks[i].guessAction.description = defaultDescription;
        }
        page.ConnectActions();
        page.icon.sprite = origin.characterIcon;
        page.icon.color = origin.characterIconColor;
        origin.entry = this;
        guess.entry = this;
    }

    public void CreateChecksMerc()
    {
        isMerc = true;
        //page.ConnectActions();
        page.icon.sprite = origin.characterIcon;
        page.icon.color = origin.characterIconColor;
    }

    private void Update()
    {
        if (!isMerc)
            CheckStats();
    }

    public void SetActiveAction(int index)
    {
        activeAction = actionChecks[index];
    }

    public void CheckStats()
    {
        // Hit points
        if (guess.hitPoints == origin.hitPoints) statChecks[StatEntries.HitPoints] = true;
        else statChecks[StatEntries.HitPoints] = false;

        // Speed
        if (guess.speed == origin.speed) statChecks[StatEntries.Speed] = true;
        else statChecks[StatEntries.Speed] = false;

        // Movement
        if (guess.movement == origin.movement) statChecks[StatEntries.Movement] = true;
        else statChecks[StatEntries.Movement] = false;

        // Resistances
        if (guess.resistances.Count == origin.resistances.Count && origin.resistances.Count > 0)
        {
            int correctHits = 0;
            foreach (Character.DamageTypes dt in guess.resistances)
            {
                if (origin.resistances.Contains(dt))
                {
                    correctHits++;
                }
            }
            if (correctHits == origin.resistances.Count) statChecks[StatEntries.Resistances] = true;
            else statChecks[StatEntries.Resistances] = false;
        }
        else if (guess.resistances.Count != origin.resistances.Count) statChecks[StatEntries.Resistances] = false;
        else if (guess.resistances.Count == origin.resistances.Count && origin.resistances.Count == 0) statChecks[StatEntries.Resistances] = true;

        // Weaknesses
        if (guess.weaknesses.Count == origin.weaknesses.Count && origin.weaknesses.Count > 0)
        {
            int correctHits = 0;
            foreach (Character.DamageTypes dt in guess.weaknesses)
            {
                if (origin.weaknesses.Contains(dt))
                {
                    correctHits++;
                }
            }
            if (correctHits == origin.weaknesses.Count) statChecks[StatEntries.Weaknesses] = true;
            else statChecks[StatEntries.Weaknesses] = false;
        }
        else if (guess.weaknesses.Count != origin.weaknesses.Count) statChecks[StatEntries.Weaknesses] = false;
        else if (guess.weaknesses.Count == origin.weaknesses.Count && origin.weaknesses.Count == 0) statChecks[StatEntries.Weaknesses] = true;

    }

    public void SetHitPoints(int value)
    {
        guess.hitPoints = value;
    }

    public void SetSpeed(int value)
    {
        guess.speed = value;
    }

    public void SetMovement(int value)
    {
        guess.movement = value;
    }

    public void SetResistance(Character.DamageTypes damageType)
    {
        if (!guess.resistances.Contains(damageType))
        {
            guess.resistances.Add(damageType);
        }
        
    }
    public void SetWeakness(Character.DamageTypes damageType)
    {
        if (!guess.weaknesses.Contains(damageType))
        {
            guess.weaknesses.Add(damageType);
        }
    }

}

public class ActionCheck
{
    public CharacterStats origin;
    public Action originalAction, guessAction;
    public float validPercent;
    public bool descriptionCorrect;

    int priority;
    Action.ActionType type;
    Debuff debuff; // Only count 1 for percentage
    Buff buff;
    Action.Position position;
    int nearTargetCount; // Dont count for percentage

    Action.Shape shape;
    int minimumHits; // Dont count for percentage
    Action.Target target; // Dont count for percentage
    Action.TargetGroup targetGroup;
    List<Action.Status> targetConditions;
    Action.TargetPriority targetPriority;
    bool canHitSelf; // Dont count for percentage
    Character.DamageTypes damageType;
    bool isCritical;

    public ActionCheck(CharacterStats o, Action g)
    {
        origin = o;
        guessAction = g;
    }

    public void FetchInfo()
    {
        priority = originalAction.actionPriority;
        type = originalAction.actionType;
        debuff = originalAction.debuff;
        buff = originalAction.buff;
        position = originalAction.position;
        nearTargetCount = originalAction.nearTargetCount;

        shape = originalAction.shape;
        minimumHits = originalAction.minimumHits;
        target = originalAction.target;
        targetGroup = originalAction.targetGroup;
        targetConditions = originalAction.targetConditions;
        targetPriority = originalAction.targetPriority;
        canHitSelf = originalAction.canHitSelf;
        damageType = originalAction.damageType;
        isCritical = originalAction.isCritical;
    }

    public void CalculateValidity()
    {
        SetComparison();
        if (originalAction == null)
        {
            Debug.LogError("Incorrect comparison, original action invalid");
            return;
        }
        FetchInfo();
        Dictionary<int, bool> flags = new Dictionary<int, bool>();

        validPercent = 0;
        if (priority == guessAction.actionPriority) validPercent += 0.1f; else flags.Add(0,true); 
        if (type == guessAction.actionType) validPercent += 0.1f;else flags.Add(1,true);
        if (buff != null && buff == guessAction.buff) validPercent += 0.1f; else flags.Add(2, true);
        if (debuff != null && debuff == guessAction.debuff) validPercent += 0.1f;
        if ((debuff == null && guessAction.debuff == null) && (buff == null && guessAction.buff == null)) validPercent += 0.1f; else flags.Add(3, true); 
        if (position == guessAction.position) validPercent += 0.1f; else flags.Add(4, true); 
        if (shape == guessAction.shape) validPercent += 0.1f; else flags.Add(5,true); 
        if (targetGroup == guessAction.targetGroup) validPercent += 0.1f;else flags.Add(6,true); 
        if (targetConditions.Count > 0)
        {
            if (originalAction.targetConditions.Contains(targetConditions[0]))
            {
                validPercent += 0.1f; 
            }
            else flags.Add(7,true);
        }
        // Use this instead of the previous once you figure out how to implement adding conditions to the list
        //if (targetConditions.Count > 0 && targetConditions.Count == guessAction.targetConditions.Count)
        //{
        //    int hitCount = 0;
        //    for (int i = 0; i < targetConditions.Count; i++)
        //    {
        //        if (guessAction.targetConditions.Contains(targetConditions[i]))
        //        {
        //            hitCount++;
        //        }
        //    }
        //    if (hitCount == targetConditions.Count) validPercent += 0.1f;
        //}
        if (targetPriority == guessAction.targetPriority) validPercent += 0.1f; else flags.Add(8,true); 
        if (damageType == guessAction.damageType) validPercent += 0.1f; else flags.Add(9,true); 
        if (isCritical == guessAction.isCritical) validPercent += 0.1f; else flags.Add(10,true); 

        Debug.Log("Validity is at " + (validPercent * 100).ToString() + "%");
        Debug.Log(flags.Count);
        foreach (KeyValuePair<int,bool> kvp in flags)
        {
            Debug.Log(kvp.Key + " is invalid");
        }
    }

    public void SetComparison()
    {
        descriptionCorrect = false;
        if (guessAction.descriptionIndex == -1) return;
        for (int i = 0; i < 4; i++)
        {
            if (origin.actions[i].descriptionIndex == guessAction.descriptionIndex)
            {
                originalAction = origin.actions[i];
                descriptionCorrect = true;
            }
        }
    }
}
