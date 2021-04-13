﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[CreateAssetMenu(fileName = "New Action", menuName = "Action")]
public class Action : ScriptableObject
{
    public string actionName;
    public string actionCode;
    public string description;

    public Character Actor
    {
        get; set;
    }
    [HideInInspector] public MonsterAI monsterAI;

    public string actionDescription;
    public int descriptionIndex = -1;
    public enum ActionType { Attack, Buff, Healing, Debuff, AttackDebuff, HealingBuff }

    public enum Position { NearEnemy, NotNearEnemy, NearAlly, NotNearAlly, Alone, Irrelevant }
    [Header("Outdated")]

    public Position position = Position.Irrelevant;
    [Tooltip("Set to true if position check should be ignored if being attacked")]
    public bool useBeingAttackedOverride;
    [Tooltip("Number of targets needed for position to be valid. Default 1.")]
    public int nearTargetCount = 1;
    
    public enum Shape { Melee, Ranged, Self, Arc, Cone, Line, Area, Pulse, All }
    //[Tooltip("When targeting ALL, use Single")]
    public enum Status { Below50, Above50, InMelee, NotInMelee, Irrelevant }
    [Tooltip("Needs to be true for the action to be chosen")]
    public List<Status> targetConditions = new List<Status>();
    [Tooltip("If this character is within the AoE of its own attack, does it get hit?")]
    public bool canHitSelf = false;

    [Tooltip("Check if this always hits self, regardless of target")]
    public bool alwaysHitsSelf = false;

    [Tooltip("Will this hurt allies unintentionally?")]
    public bool canFriendlyFire = false;

    [Tooltip("Default = 1. Only change if the action deals no damage, or is healing.")]
    public int value = 1;

    [Tooltip("Should this attack always be a critical hit?")]
    public bool isCritical = false;

    public Character.DamageTypes damageType = Character.DamageTypes.Cutting;
    public TargetGroup targetGroup = TargetGroup.Enemies;


    [Header("Targeting")]
    [Tooltip("Minimum number of hits to be valid")]
    public int minimumHits = 1;

    public enum TargetGroup { Enemies, Allies, All }
    
    public enum TargetPriority { MostHurt, MostHits, NotHurtingAlly, HasSpecificCondition, DoesntHaveSpecificCondition }
    [Tooltip("Tiebreaker if multiple targets have the same target status")]
    public TargetPriority targetPriority = TargetPriority.MostHurt;
    public Condition priorityConditionComparison;

    
        [Tooltip("A number between 1-4, determining which actions are prioritized first if multiple are available")]
    [Range(1, 4)]
    public int actionPriority = 1;


    [HideInInspector]
    public bool descriptionSet, targetingSet, outcomeSet;

    public bool isPass = false;
    public enum Condition { Acid, Armor, Bleeding, Burning, DisorientMonster, DisorientMerc, Dodge, FearMonster, FearMerc, 
        Haste, Poison, Regeneration, RootMonster, RootMerc, SlowMonster, SlowMerc, StrengthenSelf, StrengthenOther, Stun, TauntMonster, TauntMerc, Vulnerable, Weaken }
    public static readonly Dictionary<Condition, bool> ConditionIsBuff = new Dictionary<Condition, bool>()
    {
        { Condition.Acid, false },
        { Condition.Armor, true },
        { Condition.Bleeding, false },
        { Condition.Burning, false },
        { Condition.DisorientMonster, false },
        { Condition.DisorientMerc, false },
        { Condition.Dodge, true },
        { Condition.FearMonster, false },
        { Condition.FearMerc, false},
        { Condition.Haste, true },
        { Condition.Poison, false },
        { Condition.Regeneration, true },
        { Condition.RootMonster, false },
        { Condition.RootMerc, false },
        { Condition.SlowMonster, false },
        { Condition.SlowMerc, false },
        { Condition.StrengthenSelf, true },
        { Condition.StrengthenOther, true },
        { Condition.Stun, false },
        { Condition.TauntMonster, false },
        { Condition.TauntMerc, false },
        { Condition.Vulnerable, false },
        { Condition.Weaken, false }
    };
    public static readonly Dictionary<Condition, bool> ConditionIsDamageOverTime = new Dictionary<Condition, bool>()
    {
        { Condition.Acid, true },
        { Condition.Armor, false },
        { Condition.Bleeding, true },
        { Condition.Burning, true },
        { Condition.DisorientMonster, false },
        { Condition.DisorientMerc, false },
        { Condition.Dodge, false },
        { Condition.FearMonster, false },
        { Condition.FearMerc, false},
        { Condition.Haste, false },
        { Condition.Poison, true },
        { Condition.Regeneration, false},
        { Condition.RootMonster, false },
        { Condition.RootMerc, false },
        { Condition.SlowMonster, false },
        { Condition.SlowMerc, false },
        { Condition.StrengthenSelf, false },
        { Condition.StrengthenOther, false },
        { Condition.Stun, false },
        { Condition.TauntMonster, false },
        { Condition.TauntMerc, false },
        { Condition.Vulnerable, false },
        { Condition.Weaken, false }
    };
    public static readonly Dictionary<Condition, Character.DamageTypes> DamageOverTimeType = new Dictionary<Condition, Character.DamageTypes>()
    {
        { Condition.Acid, Character.DamageTypes.Acid },
        { Condition.Bleeding, Character.DamageTypes.Cutting },
        { Condition.Burning, Character.DamageTypes.Fire },
        { Condition.Poison, Character.DamageTypes.Poison },
    };

    public enum Context { NearAlly, NotNearAlly, NearEnemy, NotNearEnemy, Alone, 
        TookDamage, TookDamageOfType, TookNoDamage, DealtDamage, ReceivedHealing, ReceivedSpecificCondition, ReceivedBuff, ReceivedDebuff,
        SelfHurt, EnemyHurt, AllyHurt, SelfHasSpecificCondition, EnemyHasSpecificCondition, AllyHasSpecificCondition, SelfBeingAttacked, SelfNotBeingAttacked }
    public static readonly Dictionary<Context, List<Context>> InvalidContextPairs = new Dictionary<Context, List<Context>>()
    {
        { Context.Alone, new List<Context>(){Context.NearAlly, Context.NearEnemy, Context.NotNearAlly, Context.NotNearEnemy} },
        { Context.NearEnemy, new List<Context>(){Context.NotNearEnemy, Context.Alone} },
        { Context.NearAlly, new List<Context>(){Context.NotNearAlly, Context.Alone} },
        { Context.NotNearEnemy, new List<Context>(){Context.NearEnemy, Context.Alone} },
        { Context.NotNearAlly, new List<Context>(){Context.NearAlly, Context.Alone} },
        { Context.TookDamage, new List<Context>(){Context.TookNoDamage} },
        { Context.TookDamageOfType, new List<Context>(){Context.TookNoDamage} },
        { Context.SelfBeingAttacked, new List<Context>(){Context.SelfNotBeingAttacked} },
        { Context.SelfNotBeingAttacked, new List<Context>(){Context.SelfBeingAttacked} }
    };
    public enum Targeting { Character, Ground }
    [Header("Primary effect")]
    [Tooltip("A list of contextual triggers that need to be true for the primary action to be valid.")]
    public List<ContextInfo> primaryContext = new List<ContextInfo>();
    [Tooltip("What the primary action targets.")]
    public Targeting primaryTargeting = Targeting.Character;
    public Shape primaryShape = Shape.Melee;
    public TargetGroup primaryTargetGroup = TargetGroup.Enemies;
    [Tooltip("Output of the primary action.")]
    public List<OutputInfo> primaryOutput = new List<OutputInfo>();

    [Header("Secondary effect")]
    [Tooltip("What the secondary action targets.")]
    public Targeting secondaryTargeting = Targeting.Character;
    public Shape secondaryShape = Shape.Self;
    public TargetGroup secondaryTargetGroup = TargetGroup.Allies;
    public enum Output { Damage, Healing, Condition, Movement }
    public List<OutputInfo> secondaryOutput = new List<OutputInfo>();
    public class Target
    {
        public Node Node
        {
            get;set;
        }
        public Character Character
        {
            get;set;
        }
        public List<Node> AffectedNodes
        {
            get;set;
        }
        public List<Character> AffectedCharacters
        {
            get;set;
        }
    }

    public CombatAction CombatAction(BattlefieldPositionInfo bpi, bool guess)
    {
        CombatAction output = new CombatAction(Actor, this);
        if (!ActionValidation(bpi, guess)) return output;
        output.valid = true;
        ShapeTest primaryTest = NodeTarget(bpi, true);
        ShapeTest secondaryTest = null;
        if (secondaryOutput.Count > 0) secondaryTest = NodeTarget(bpi, false);
        if (primaryTest.valid)
        {
            if (primaryTest.potentialTargets.Count > 1)
            {
                Target t = BestTarget(primaryTest.potentialTargets);
                output.primaryTarget = t;
            }
            else if (primaryTest.potentialTargets.Count == 1)
            {
                output.primaryTarget = new Target();
                output.primaryTarget.Character = primaryTest.potentialTargets[0].targetCharacter;
                output.primaryTarget.Node = primaryTest.potentialTargets[0].targetNode;
                output.primaryTarget.AffectedCharacters = primaryTest.potentialTargets[0].affectedCharacters;
                output.primaryTarget.AffectedNodes = primaryTest.potentialTargets[0].affectedNodes;
            }
            else
            {
                output.valid = false;
                return output;
            }
            if (secondaryTest!= null && secondaryTest.valid)
            {
                output.secondaryTargetGroup = secondaryTargetGroup;
                if (secondaryTest.potentialTargets.Count > 1)
                {
                    Target t = BestTarget(secondaryTest.potentialTargets);
                    output.secondaryTarget = t;
                }
                else
                {
                    output.secondaryTarget = new Target();
                    output.secondaryTarget.Character = secondaryTest.potentialTargets[0].targetCharacter;
                    output.secondaryTarget.Node = secondaryTest.potentialTargets[0].targetNode;
                    output.secondaryTarget.AffectedCharacters = secondaryTest.potentialTargets[0].affectedCharacters;
                    output.secondaryTarget.AffectedNodes = secondaryTest.potentialTargets[0].affectedNodes;
                }
            }
        }

        return output;
    }

    public bool ActionValidation(BattlefieldPositionInfo bpi, bool guess)
    {
        if (guess) return true;
        int count = 0;
        for (int i = 0; i < primaryContext.Count; i++)
        {
            primaryContext[i].actor = Actor;
            if (primaryContext[i].ContextValid(bpi)) count++;
        }
        if (count != primaryContext.Count) return false;

        else return true;
    }

    private ShapeTest NodeTarget(BattlefieldPositionInfo bpi, bool primary)
    {
        ShapeTest test = new ShapeTest(this, Actor, primary ? minimumHits : 0, primary ? primaryTargetGroup : secondaryTargetGroup, primary ? primaryTargeting : secondaryTargeting);
        ShapeTest result = new ShapeTest(this, Actor);
        List<Character> possibleTargets = new List<Character>();
        switch (primary ? primaryTargetGroup : secondaryTargetGroup)
        {
            case TargetGroup.Enemies:
                foreach(Character c in CombatManager.actors)
                {
                    if (!Character.AllyOrEnemy(Actor, c)) possibleTargets.Add(c);
                }
                break;
            case TargetGroup.Allies:
                foreach (Character c in CombatManager.actors)
                {
                    if (Character.AllyOrEnemy(Actor, c)) possibleTargets.Add(c);
                }
                break;
            case TargetGroup.All:
                possibleTargets = CombatManager.actors;
                break;
        }
        switch (primary ? primaryShape : secondaryShape)
        {
            case Shape.Self:
                possibleTargets.Clear();
                possibleTargets.Add(Actor);
                foreach (Character c in possibleTargets)
                {
                    result.potentialTargets.Add(new TargetInfo(Actor, new List<Character>() { c }));
                }
                result.valid = true;
                break;
            case Shape.Melee:
                possibleTargets.Clear();
                switch (primary ? primaryTargetGroup : secondaryTargetGroup)
                {
                    case TargetGroup.Enemies:
                        foreach (Character c in bpi.enemiesInMelee) possibleTargets.Add(c);
                        break;
                    case TargetGroup.Allies:
                        foreach (Character c in bpi.alliesInMelee) possibleTargets.Add(c);
                        break;
                    case TargetGroup.All:
                        foreach (Character c in bpi.enemiesInMelee) possibleTargets.Add(c);
                        foreach (Character c in bpi.alliesInMelee) possibleTargets.Add(c);
                        break;
                }
                if (possibleTargets.Count > 0)
                {
                    result.valid = true;
                    foreach (Character c in possibleTargets)
                    {
                        result.potentialTargets.Add(new TargetInfo(c, new List<Character>() { c }));
                    }
                }
                break;
            case Shape.Ranged:
                switch(primary ? primaryTargetGroup : secondaryTargetGroup)
                {
                    case TargetGroup.Enemies:
                        foreach (Character c in bpi.enemiesInMelee) possibleTargets.Remove(c);
                        break;
                    case TargetGroup.Allies:
                        foreach (Character c in bpi.alliesInMelee) possibleTargets.Remove(c);
                        break;
                    case TargetGroup.All:
                        foreach (Character c in bpi.alliesInMelee) possibleTargets.Remove(c);
                        foreach (Character c in bpi.enemiesInMelee) possibleTargets.Remove(c);
                        break;
                }
                if (possibleTargets.Count > 0)
                {
                    result.valid = true;
                    foreach (Character c in possibleTargets)
                    {
                        result.potentialTargets.Add(new TargetInfo(c, new List<Character>() { c }));
                    }
                }
                break;
            case Shape.Arc:
                result = CombatGrid.ArcTest(test);                
                break;
            case Shape.Cone:
                result = CombatGrid.ConeTest(test);
                break;
            case Shape.Line:
                result = CombatGrid.LineTest(test);
                break;
            case Shape.Area:
                result = CombatGrid.AreaTest(test);
                break;
            case Shape.Pulse:
                result = CombatGrid.PulseTest(test);
                break;
            case Shape.All:
                result.valid = true;
                result.potentialTargets.Add(new TargetInfo(Actor, possibleTargets));
                break;
        }
        return result;
    }

    private Target BestTarget(List<TargetInfo> ti)
    {
        Target output = new Target();

        float damageTaken = -1;
        int hits = 0;

        switch (targetPriority)
        {
            case TargetPriority.MostHurt:
                for (int i = 0; i < ti.Count; i++)
                {
                    for (int j = 0; j < ti[i].affectedCharacters.Count; j++)
                    {
                        if (ti[i].affectedCharacters[j].damageTaken > damageTaken)
                        {
                            damageTaken = ti[i].affectedCharacters[j].damageTaken;
                            output.Node = ti[i].targetNode;
                            output.Character = ti[i].targetCharacter;
                            output.AffectedNodes = ti[i].affectedNodes;
                            output.AffectedCharacters = ti[i].affectedCharacters;
                        }
                    }
                }
                if (output.Character == null)
                {
                    output.Node = ti[0].targetNode;
                    output.Character = ti[0].targetCharacter;
                    output.AffectedCharacters = ti[0].affectedCharacters;
                    output.AffectedNodes = ti[0].affectedNodes;
                }
                break;
            case TargetPriority.MostHits:
                for (int i = 0; i < ti.Count; i++)
                {
                    if (ti[i].affectedCharacters.Count > hits)
                    {
                        hits = ti[i].affectedCharacters.Count;
                        output.Node = ti[i].targetNode;
                        output.Character = ti[i].targetCharacter;
                        output.AffectedNodes = ti[i].affectedNodes;
                        output.AffectedCharacters = ti[i].affectedCharacters;
                    }
                }
                break;
            case TargetPriority.NotHurtingAlly:
                bool flag = false;
                for (int i = 0; i < ti.Count; i++)
                {
                    for (int j = 0; j < ti[i].affectedCharacters.Count; j++)
                    {
                        if (Character.AllyOrEnemy(Actor, ti[i].affectedCharacters[j]))
                        {
                            flag = false;
                            break;
                        }
                        flag = true;
                    }
                    if (flag)
                    {
                        output.Node = ti[i].targetNode;
                        output.Character = ti[i].targetCharacter;
                        output.AffectedNodes = ti[i].affectedNodes;
                        output.AffectedCharacters = ti[i].affectedCharacters;
                    }
                }
                break;
            case TargetPriority.HasSpecificCondition:
                bool flag1 = false;
                for (int i = 0; i < ti.Count; i++)
                {
                    for (int j = 0; j < ti[i].affectedCharacters.Count; j++)
                    {
                        if (ti[i].affectedCharacters[j].Conditions.ContainsKey(priorityConditionComparison))
                        {
                            output.Node = ti[i].targetNode;
                            output.Character = ti[i].targetCharacter;
                            output.AffectedNodes = ti[i].affectedNodes;
                            output.AffectedCharacters = ti[i].affectedCharacters;
                            flag1 = true;
                            break;
                        }
                    }
                    if (flag1) break;
                }
                break;
            case TargetPriority.DoesntHaveSpecificCondition:
                bool flag2 = false;
                for (int i = 0; i < ti.Count; i++)
                {
                    for (int j = 0; j < ti[i].affectedCharacters.Count; j++)
                    {
                        if (ti[i].affectedCharacters[j].Conditions.ContainsKey(priorityConditionComparison))
                        {
                            break;
                        }
                        flag2 = true;
                    }
                    if (flag2)
                    {
                        output.Node = ti[i].targetNode;
                        output.Character = ti[i].targetCharacter;
                        output.AffectedNodes = ti[i].affectedNodes;
                        output.AffectedCharacters = ti[i].affectedCharacters;
                        break;
                    }
                }
                break;
        }

        return output;
    }
    #region Old Validation
    /*
    public Node ActionValid(BattlefieldPositionInfo bpi, bool basedOnGuess)
    {
        List<Node> possibleTargets = new List<Node>();
        Node bestTarget = null;
        bool flag1 = false;
        bool flag2 = false;
        bool flag3 = false;

        // Check Position. If it is valid, flag1 is true
        switch (position)
        {
            case Position.Alone:
                if (bpi.alliesInMelee.Count == 0 && bpi.enemiesInMelee.Count == 0) flag1 = true;
               
               
                break;
            case Position.NearAlly:
                if (bpi.alliesInMelee.Count >= nearTargetCount) flag1 = true;
                
                break;
            case Position.NotNearAlly:
                if (bpi.alliesInMelee.Count == 0) flag1 = true;
               
                break;
            case Position.NearEnemy:
                if (bpi.enemiesInMelee.Count >= nearTargetCount) flag1 = true;
                break;
            case Position.NotNearEnemy:
                if (bpi.enemiesInMelee.Count == 0) flag1 = true;
                break;
            case Position.Irrelevant:
                flag1 = true;
                break;
        }

        // Check if the shape can hit the required number of targets. If it can, flag2 is true
        if (flag1 || basedOnGuess)
        {
            ShapeTest test = new ShapeTest(this, bpi.origin, minimumHits, targetGroup == TargetGroup.Enemies);
            ShapeTest result;
            switch (shape)
            {
                case Shape.Single:
                    flag2 = true;
                    if (target == Target.Self) possibleTargets.Add(bpi.origin.movement.currentNode);
                    else
                    {
                        foreach (Character actor in CombatManager.actors)
                        {
                            if (actor.alive)
                            {
                                switch (targetGroup)
                                {
                                    case TargetGroup.Allies:
                                        if (actor != bpi.origin && Character.AllyOrEnemy(bpi.origin, actor))
                                        {
                                            possibleTargets.Add(actor.movement.currentNode);
                                        }
                                        break;
                                    case TargetGroup.Enemies:
                                        if (!Character.AllyOrEnemy(bpi.origin, actor))
                                        {
                                            possibleTargets.Add(actor.movement.currentNode);
                                        }
                                        break;
                                    case TargetGroup.All:
                                        possibleTargets.Add(actor.movement.currentNode);
                                        break;
                                }
                            }
                        }
                    }
                    break;
                case Shape.Arc:                    
                    result = CombatGrid.ArcTest(test);
                    if (result.valid)
                    {
                        bestTarget = result.targetNode;
                        flag2 = true;
                    }
                    break;
                case Shape.Cone:
                    result = CombatGrid.ConeTest(test);
                    if (result.valid)
                    {
                        bestTarget = result.targetNode;
                        flag2 = true;
                    }
                    break;
                case Shape.Line:
                    result = CombatGrid.LineTest(test);
                    if (result.valid)
                    {
                        bestTarget = result.targetNode;
                        flag2 = true;
                    }
                    break;
                case Shape.ThreeByThree:
                    if (target == Target.Self)
                    {
                        result = CombatGrid.PointBlankAoETest(test);
                        if (result.valid)
                        {
                            bestTarget = result.targetNode;
                            flag2 = true;
                        }
                    }
                    else
                    {
                        result = CombatGrid.ThreeByThreeTest(test);
                        if (result.valid)
                        {
                            bestTarget = result.targetNode;
                            flag2 = true;
                        }
                    }
                    break;
                case Shape.All:
                    flag2 = true;
                    foreach (Character actor in CombatManager.actors)
                    {
                        if (actor.alive)
                        {
                            switch (targetGroup)
                            {
                                case TargetGroup.Allies:
                                    if (actor != bpi.origin && Character.AllyOrEnemy(bpi.origin, actor))
                                    {
                                        possibleTargets.Add(actor.movement.currentNode);
                                    }
                                    else if (actor == bpi.origin)
                                    {
                                        possibleTargets.Add(actor.movement.currentNode);
                                    }
                                    break;
                                case TargetGroup.Enemies:
                                    if (!Character.AllyOrEnemy(bpi.origin, actor))
                                    {
                                        possibleTargets.Add(actor.movement.currentNode);
                                    }
                                    break;
                                case TargetGroup.All:
                                    possibleTargets.Add(actor.movement.currentNode);
                                    break;
                            }
                        }
                    }
                    break;
            }
        }
        // Remove dead targets
        for (int i = 0; i < possibleTargets.Count; i++)
        {
            if (!possibleTargets[i].occupant.alive)
            {
                possibleTargets.RemoveAt(i);
                i--;
            }
        }
        // Find possible targets based on target status. If none are found, flag3 is false
        if (flag2  || basedOnGuess)
        {

            foreach (Status targetStatus in targetConditions)
            {
                switch (targetStatus)
                {
                    case Status.Above50:
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            Character target = possibleTargets[i].occupant;
                            if (target != null && target.alive && target.currentHitpoints <= target.stats.hitPoints / 2)
                            {
                                possibleTargets.RemoveAt(i);
                                i--;
                            }
                        }
                        break;
                    case Status.Below50:
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            Character target = possibleTargets[i].occupant;
                            if (target != null && target.alive && target.currentHitpoints >= target.stats.hitPoints / 2)
                            {
                                possibleTargets.RemoveAt(i);
                                i--;
                            }
                        }
                        break;
                    case Status.InMelee:
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            Character target = possibleTargets[i].occupant;
                            switch (targetGroup)
                            {
                                case TargetGroup.Allies:
                                    if (target != null && !bpi.alliesInMelee.Contains(target))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                                case TargetGroup.Enemies:
                                    if (target != null && !bpi.enemiesInMelee.Contains(target))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                                case TargetGroup.All:
                                    if (target != null && !bpi.alliesInMelee.Contains(target) && !bpi.enemiesInMelee.Contains(target))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                            }
                        }
                        break;
                    case Status.NotInMelee:
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            Character target = possibleTargets[i].occupant;
                            switch (targetGroup)
                            {
                                case TargetGroup.Allies:
                                    if (target != null && bpi.alliesInMelee.Contains(target))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                                case TargetGroup.Enemies:
                                    if (target != null && bpi.enemiesInMelee.Contains(target))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                                case TargetGroup.All:
                                    if (target != null && (bpi.alliesInMelee.Contains(target) || bpi.enemiesInMelee.Contains(target)))
                                    {
                                        possibleTargets.RemoveAt(i);
                                        i--;
                                    }
                                    break;
                            }
                        }
                        break;
                    case Status.Irrelevant:
                        flag3 = true;
                        break;
                }
            }
            if (possibleTargets.Count > 0) flag3 = true;
        }

        // Check priority for tiebreaking
        if (possibleTargets.Count > 0)
        {
            float dist = 0f, distMin = 10000, distMax = 0f;
            float hp = 0, hpMin = 10000, hpMax = 0;
            switch (targetPriority)
            {
                case TargetPriority.Closest:
                    foreach (Node node in possibleTargets)
                    {
                        dist = (node.coordinate - bpi.origin.position).sqrMagnitude;
                        if (dist < distMin)
                        {
                            bestTarget = node;
                            distMin = dist;
                        }
                    }
                    break;
                case TargetPriority.Farthest:
                    foreach (Node node in possibleTargets)
                    {
                        dist = (node.coordinate - bpi.origin.position).sqrMagnitude;
                        if (dist > distMax)
                        {
                            bestTarget = node;
                            distMax = dist;
                        }
                    }
                    break;
                case TargetPriority.HighestHPCurrent:
                    foreach (Node node in possibleTargets)
                    {
                        if (node.occupant != null)
                        {
                            hp = node.occupant.currentHitpoints;
                            if (hp > hpMax)
                            {
                                bestTarget = node;
                                hpMax = hp;
                            }
                        }
                    }
                    break;
                case TargetPriority.HighestHPPercent:
                    foreach (Node node in possibleTargets)
                    {
                        if (node.occupant != null)
                        {
                            hp = node.occupant.currentHitpoints / node.occupant.stats.hitPoints;
                            if (hp > hpMax)
                            {
                                bestTarget = node;
                                hpMax = hp;
                            }
                        }
                    }
                    break;
                case TargetPriority.LowestHPCurrent:
                    foreach (Node node in possibleTargets)
                    {
                        if (node.occupant != null)
                        {
                            hp = node.occupant.currentHitpoints;
                            if (hp < hpMin)
                            {
                                bestTarget = node;
                                hpMin = hp;
                            }
                        }
                    }
                    break;
                case TargetPriority.lowestHPPercent:
                    foreach (Node node in possibleTargets)
                    {
                        if (node.occupant != null)
                        {
                            hp = node.occupant.currentHitpoints / node.occupant.stats.hitPoints;
                            if (hp < hpMin)
                            {
                                bestTarget = node;
                                hpMin = hp;
                            }
                        }
                    }
                    break;
                case TargetPriority.HasSameDebuff:
                    //Fix this to be similar to the one below
                    foreach (Node node in possibleTargets)
                    {
                        Character pt = node.occupant;
                        if (pt != null)
                        {
                            if(pt.debuffs.Count > 0)
                            {
                                for (int i = 0; i < pt.debuffs.Count; i++)
                                {
                                    if (pt.debuffs[i].debuffType == debuff.debuffType)
                                    {
                                        if (pt.debuffs[i].debuffType == Debuff.DebuffType.DamageOverTime && pt.debuffs[i].damageType == debuff.damageType)
                                        {
                                            bestTarget = node;
                                            break;
                                        }
                                        else if (pt.debuffs[i].debuffType == Debuff.DebuffType.Control && pt.debuffs[i].controlType == debuff.controlType)
                                        {
                                            bestTarget = node;
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    break;
                case TargetPriority.DoesntHaveSameDebuff:
                    List<Node> sameDebuffTargets = new List<Node>();
                    if (possibleTargets.Count > 0)
                    {
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            Character pt = possibleTargets[i].occupant;
                            if (pt != null)
                            {
                                if (debuff != null)
                                {
                                    if (pt.debuffs.Count > 0)
                                    {
                                        bool flag = false;
                                        foreach (Debuff d in pt.debuffs)
                                        {
                                            if ((d.debuffType == debuff.debuffType && d.debuffType == Debuff.DebuffType.DamageOverTime) && d.damageType == debuff.damageType)
                                            {
                                                flag = true;
                                            }
                                            else if ((d.debuffType == debuff.debuffType && d.debuffType == Debuff.DebuffType.Control) && d.controlType == debuff.controlType)
                                            {
                                                flag = true;
                                            }
                                        }
                                        if (flag)
                                        {
                                            sameDebuffTargets.Add(possibleTargets[i]);
                                            //possibleTargets.RemoveAt(i);
                                            //i--;
                                        }
                                    }
                                }
                                else if (buff != null)
                                {
                                    if (pt.buffs.Count > 0)
                                    {
                                        bool flag = false;
                                        foreach (Buff b in pt.buffs)
                                        {
                                            if (b.buffType == buff.buffType)
                                            {
                                                flag = true;
                                            }
                                        }
                                        if (flag) sameDebuffTargets.Add(possibleTargets[i]);
                                    }
                                }
                            }
                        }
                    }
                    else if (bestTarget != null)
                    {
                        bool flag = false;

                        if (debuff != null)
                        {
                            if (bestTarget.occupant.debuffs.Count > 0)
                            {
                                foreach (Debuff d in bestTarget.occupant.debuffs)
                                {
                                    if ((d.debuffType == debuff.debuffType && d.debuffType == Debuff.DebuffType.DamageOverTime) && d.damageType == debuff.damageType)
                                    {
                                        flag = true;
                                    }
                                    else if ((d.debuffType == debuff.debuffType && d.debuffType == Debuff.DebuffType.Control) && d.controlType == debuff.controlType)
                                    {
                                        flag = true;
                                    }
                                }
                            }
                        }
                        else if (buff != null)
                        {
                            if (bestTarget.occupant.buffs.Count > 0)
                            {
                                foreach (Buff b in bestTarget.occupant.buffs)
                                {
                                    if (b.buffType == buff.buffType)
                                    {
                                        flag = true;
                                    }
                                }
                            }
                        }
                        if (flag) flag3 = false;
                    }
                    if (sameDebuffTargets.Count == possibleTargets.Count && possibleTargets.Count > 0) bestTarget = possibleTargets[0];
                    else if (sameDebuffTargets.Count < possibleTargets.Count && sameDebuffTargets.Count != 0)
                    {
                        for (int i = 0; i < possibleTargets.Count; i++)
                        {
                            if (sameDebuffTargets.Contains(possibleTargets[i]))
                            {
                                possibleTargets.RemoveAt(i);
                                i--;
                            }
                        }
                        bestTarget = possibleTargets[0];
                    }
                    else if (possibleTargets.Count > 0 && sameDebuffTargets.Count == 0) bestTarget = possibleTargets[0];
                    else flag3 = false;
                    break;
                case TargetPriority.None:
                    break;
            }
        }
        if (!flag3 && useBeingAttackedOverride)
        {
            if (CombatManager.CharacterIsBeingAttacked(bpi.origin))
            {
                bestTarget = bpi.origin.movement.currentNode;
                flag3 = true;
            }
        }

        if (flag3 && bestTarget == null && possibleTargets.Count > 0)
        {
            int index = GameManager.seed[CombatManager.instance.roundCount] % possibleTargets.Count;
            bestTarget = possibleTargets[index];
        }
        else if (flag3 && bestTarget == null && possibleTargets.Count == 0) flag3 = false;
        else if ((!flag3 && bestTarget != null) && basedOnGuess) flag3 = true;

        if (flag3)
            return bestTarget;
        else
            return null;
    }
    */
#endregion

}

[Serializable]
public class ContextInfo 
{
    [HideInInspector] public Character actor;
    public Action.Context context;
    public int value;
    public Action.Condition condition;
    public Character.DamageTypes damageType;
    [HideInInspector]public Entry.Difficulty difficulty;

    public bool ContextValid(BattlefieldPositionInfo bpi)
    {
        bool output = false;
        switch (context)
        {
            // Positions
            case Action.Context.NearAlly:
                output = bpi.alliesInMelee.Count >= value;
                break;
            case Action.Context.NearEnemy:
                output = bpi.enemiesInMelee.Count >= value;
                break;
            case Action.Context.NotNearAlly:
                output = bpi.alliesInMelee.Count == 0;
                break;
            case Action.Context.NotNearEnemy:
                output = bpi.enemiesInMelee.Count == 0;
                break;
            case Action.Context.Alone:
                output = bpi.alliesInMelee.Count == 0 && bpi.enemiesInMelee.Count == 0;
                break;
            // Memory
            case Action.Context.DealtDamage:
                output = actor.memory.DamageDealt > 0f;
                break;
            case Action.Context.TookDamage:
                output = actor.memory.DamageTaken > 0f;
                break;
            case Action.Context.TookDamageOfType:
                output = actor.memory.DamageTypesTaken.Contains(damageType);
                break;
            case Action.Context.TookNoDamage:
                output = actor.memory.DamageTaken == 0;
                break;
            case Action.Context.ReceivedSpecificCondition:
                output = actor.memory.ConditionsGained.Contains(condition);
                break;
            case Action.Context.ReceivedBuff:
                output = actor.memory.ReceivedBuff;
                break;
            case Action.Context.ReceivedDebuff:
                output = actor.memory.ReceivedDebuff;
                break;
            case Action.Context.ReceivedHealing:
                output = actor.memory.HealingReceived > 0f;
                break;
            // Status
            case Action.Context.SelfHasSpecificCondition:
                break;
            case Action.Context.EnemyHasSpecificCondition:
                break;
            case Action.Context.AllyHasSpecificCondition:
                break;
            case Action.Context.SelfHurt:
                output = actor.damageTaken > (actor.stats.hitPoints / 2);
                break;
            case Action.Context.EnemyHurt:
                for (int i = 0; i < CombatManager.actors.Count; i++)
                {
                    if(!Character.AllyOrEnemy(actor, CombatManager.actors[i]))
                    {
                        if (CombatManager.actors[i].damageTaken > (CombatManager.actors[i].stats.hitPoints / 2))
                        {
                            output = true;
                            break;
                        }
                    }
                }
                break;
            case Action.Context.AllyHurt:
                for (int i = 0; i < CombatManager.actors.Count; i++)
                {
                    if (actor != CombatManager.actors[i])
                    {
                        if (Character.AllyOrEnemy(actor, CombatManager.actors[i]))
                        {
                            if (CombatManager.actors[i].damageTaken > (CombatManager.actors[i].stats.hitPoints / 2))
                            {
                                output = true;
                                break;
                            }
                        }
                    }
                }
                break;
        }
        return output;
    }
}

[Serializable]
public class OutputInfo 
{
    public Action.Output output;
    [Tooltip("The number of condition stacks, spaces moved, or the healing value.")]
    public int value;
    [Tooltip("The condition applied, if any.")]
    public Action.Condition condition;
    [Tooltip("The damage dealt by the attack is critical. Does not apply to healing or conditions")]
    public bool critical;
    [Tooltip("The damage type associated with the attack, not the condition. Set to healing if healing.")]
    public Character.DamageTypes damageType;
    [Tooltip("Whether movement moves the character towards or away from the target/user.")]
    public bool towards;
    [HideInInspector]public Entry.Difficulty difficulty;
}
