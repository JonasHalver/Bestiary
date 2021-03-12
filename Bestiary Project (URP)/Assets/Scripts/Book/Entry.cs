﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entry : MonoBehaviour
{
    public Character origin;
    public Character guess;

    public Action activeAction;

    public enum StatEntries { HitPoints, Speed, Movement, Resistances, Weaknesses }

    public Dictionary<StatEntries, bool> statChecks = new Dictionary<StatEntries, bool>();

    public List<ActionCheck> actionChecks = new List<ActionCheck>();

    public Entry (Character _origin)
    {
        origin = _origin;
        Character character = new Character();
        guess = character;
    }

    private void Awake()
    {
        statChecks.Add(StatEntries.HitPoints, false); statChecks.Add(StatEntries.Speed, false); statChecks.Add(StatEntries.Movement, false);
        statChecks.Add(StatEntries.Resistances, false); statChecks.Add(StatEntries.Weaknesses, false);
        for (int i = 0; i < 4; i++)
        {
            actionChecks.Add(new ActionCheck(origin.stats.actions[i], ScriptableObject.CreateInstance<Action>()));
        }
    }

    private void Update()
    {
        CheckStats();
    }

    public void CheckStats()
    {
        // Hit points
        if (guess.stats.hitPoints == origin.stats.hitPoints) statChecks[StatEntries.HitPoints] = true;
        else statChecks[StatEntries.HitPoints] = false;

        // Speed
        if (guess.stats.speed == origin.stats.speed) statChecks[StatEntries.Speed] = true;
        else statChecks[StatEntries.Speed] = false;

        // Movement
        if (guess.stats.movement == origin.stats.movement) statChecks[StatEntries.Movement] = true;
        else statChecks[StatEntries.Movement] = false;

        // Resistances
        if (guess.stats.resistances.Count == origin.stats.resistances.Count && origin.stats.resistances.Count > 0)
        {
            int correctHits = 0;
            foreach (Character.DamageTypes dt in guess.stats.resistances)
            {
                if (origin.stats.resistances.Contains(dt))
                {
                    correctHits++;
                }
            }
            if (correctHits == origin.stats.resistances.Count) statChecks[StatEntries.Resistances] = true;
            else statChecks[StatEntries.Resistances] = false;
        }
        else if (guess.stats.resistances.Count != origin.stats.resistances.Count) statChecks[StatEntries.Resistances] = false;
        else if (guess.stats.resistances.Count == origin.stats.resistances.Count && origin.stats.resistances.Count == 0) statChecks[StatEntries.Resistances] = true;

        // Weaknesses
        if (guess.stats.weaknesses.Count == origin.stats.weaknesses.Count && origin.stats.weaknesses.Count > 0)
        {
            int correctHits = 0;
            foreach (Character.DamageTypes dt in guess.stats.weaknesses)
            {
                if (origin.stats.weaknesses.Contains(dt))
                {
                    correctHits++;
                }
            }
            if (correctHits == origin.stats.weaknesses.Count) statChecks[StatEntries.Weaknesses] = true;
            else statChecks[StatEntries.Weaknesses] = false;
        }
        else if (guess.stats.weaknesses.Count != origin.stats.weaknesses.Count) statChecks[StatEntries.Weaknesses] = false;
        else if (guess.stats.weaknesses.Count == origin.stats.weaknesses.Count && origin.stats.weaknesses.Count == 0) statChecks[StatEntries.Weaknesses] = true;

    }

    public void SetHitPoints(int value)
    {
        guess.stats.hitPoints = value;
    }

    public void SetSpeed(int value)
    {
        guess.stats.speed = value;
    }

    public void SetMovement(int value)
    {
        guess.stats.movement = value;
    }

    public void SetResistance(Character.DamageTypes damageType)
    {
        if (!guess.stats.resistances.Contains(damageType))
        {
            guess.stats.resistances.Add(damageType);
        }
        
    }
    public void SetWeakness(Character.DamageTypes damageType)
    {
        if (!guess.stats.weaknesses.Contains(damageType))
        {
            guess.stats.weaknesses.Add(damageType);
        }
    }
}

public class ActionCheck
{
    public Action originalAction, guessAction;
    public float validPercent;

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

    public ActionCheck(Action o, Action g)
    {
        originalAction = o;
        guessAction = g;
        FetchInfo();
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
        validPercent = 0;
        if (priority == guessAction.actionPriority) validPercent += 0.1f;
        if (type == guessAction.actionType) validPercent += 0.1f;
        if (buff != null && buff == guessAction.buff) validPercent += 0.1f;
        if (debuff != null && debuff == guessAction.debuff) validPercent += 0.1f;
        if (position == guessAction.position) validPercent += 0.1f;
        if (shape == guessAction.shape) validPercent += 0.1f;
        if (targetGroup == guessAction.targetGroup) validPercent += 0.1f;
        if (targetConditions.Count > 0 && targetConditions.Count == guessAction.targetConditions.Count)
        {
            int hitCount = 0;
            for (int i = 0; i < targetConditions.Count; i++)
            {
                if (guessAction.targetConditions.Contains(targetConditions[i]))
                {
                    hitCount++;
                }
            }
            if (hitCount == targetConditions.Count) validPercent += 0.1f;
        }
        if (targetPriority == guessAction.targetPriority) validPercent += 0.1f;
        if (damageType == guessAction.damageType) validPercent += 0.1f;
        if (isCritical == guessAction.isCritical) validPercent += 0.1f;
    }
}