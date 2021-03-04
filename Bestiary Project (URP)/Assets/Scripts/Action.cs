using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Action", menuName = "Action")]
public class Action : ScriptableObject
{
    public string actionName;
    public string description;
    public enum ActionType { Attack, Buff, Healing, Debuff, AttackDebuff, HealingBuff }
    public ActionType actionType = ActionType.Attack;

    [Tooltip("A number between 1-4, determining which actions are prioritized first if multiple are available")]
    [Range(1, 4)]
    public int actionPriority = 1;

    public Debuff debuff;

    public Buff buff;

    public enum Position { NearEnemy, NotNearEnemy, NearAlly, NotNearAlly, Alone, Irrelevant }
    public Position position = Position.Irrelevant;
    [Tooltip("Set to true if position check should be ignored if being attacked")]
    public bool useBeingAttackedOverride;
    [Tooltip("Number of targets needed for position to be valid. Default 1.")]
    public int nearTargetCount = 1;

    public enum Shape { Single, Arc, Cone, Line, ThreeByThree }
    [Tooltip("When targeting ALL, use Single")]
    [Header("Targeting")]
    public Shape shape = Shape.Single;

    [Tooltip("Minimum number of hits to be valid")]
    public int minimumHits = 1;

    public enum Target { Self, Character, Ground, All }
    [Tooltip("Where the center of the attack is.")]
    public Target target = Target.Character;

    public enum TargetGroup { Enemies, Allies, All }
    public TargetGroup targetGroup = TargetGroup.Enemies;
    
    public enum Status { Below50, Above50, InMelee, NotInMelee, Irrelevant }
    [Tooltip("Needs to be true for the action to be chosen")]
    public List<Status> targetConditions = new List<Status>();

    public enum TargetPriority { None, LowestHPCurrent, lowestHPPercent, HighestHPCurrent, HighestHPPercent, Closest, Farthest, HasSameDebuff, DoesntHaveSameDebuff }
    [Tooltip("Tiebreaker if multiple targets have the same target status")]
    public TargetPriority targetPriority = TargetPriority.None;

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

    [Header("Damage Type")]
    public Character.DamageTypes damageType = Character.DamageTypes.Cutting;

    public Node ActionValid(BattlefieldPositionInfo bpi)
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
        if (flag1)
        {
            ShapeTest test = new ShapeTest(this, bpi.origin, minimumHits, targetGroup == TargetGroup.Enemies);
            ShapeTest result;
            switch (shape)
            {
                case Shape.Single:
                    flag2 = true;
                    if (target == Target.Self) bestTarget = bpi.origin.movement.currentNode;
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
            }
        }

        // Find possible targets based on target status. If none are found, flag3 is false
        if (flag2)
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
        if (possibleTargets.Count > 1)
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
                    for (int i = 0; i < possibleTargets.Count; i++)
                    {
                        Character pt = possibleTargets[i].occupant;
                        if (pt != null)
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
        if (!flag3 && useBeingAttackedOverride) flag3 = true;

        if (flag3 && bestTarget == null && possibleTargets.Count > 0)
        { 
            bool flag4 = false;
            for (int i = 0; i < possibleTargets.Count; i++)
            {
                if (possibleTargets[i] != null)
                {
                    bestTarget = possibleTargets[i];
                    flag4 = true;
                    break;
                }
            }
            if (!flag4)
            {
                flag3 = false;
            }
        }
        else if (flag3 && bestTarget == null && possibleTargets.Count == 0) flag3 = false;

        if (flag3)
            return bestTarget;
        else
            return null;
    }
}
