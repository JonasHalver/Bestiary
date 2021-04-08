using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class MonsterAI : MonoBehaviour
{
    [HideInInspector] public Character character;
    [HideInInspector] public List<EnemyEvaluation> evaluations = new List<EnemyEvaluation>();
    [HideInInspector] public List<GridSpaceAssessment> assessments = new List<GridSpaceAssessment>();
    private Character relentlessTarget;
    public Character RelentlessTarget
    {
        get
        {
            if (relentlessTarget == null)
            {
                relentlessTarget = evaluations[Random.Range(0, evaluations.Count)].Enemy;
            }
            return relentlessTarget;
        }
        set
        {
            relentlessTarget = value;
        }
    }
    public Node Destination
    {
        get; private set;
    }

    private LastRoundMemory memory;    

    public Node SafestNode
    {
        get 
        { 
            if (assessments.Count > 0)
            {
                assessments.Sort((a1, a2) => a1.DangerValue.CompareTo(a2.DangerValue));
                return assessments[0].Space;
            }
            else
            {
                return null;
            }
        }
    }
    public Node MostDangerousNode
    {
        get
        {
            if (assessments.Count > 0)
            {
                assessments.Sort((a2, a1) => a1.DangerValue.CompareTo(a2.DangerValue));
                return assessments[0].Space;
            }
            else
            {
                return null;
            }
        }
    }
    public Node SafestAnyMeleeNode
    {
        get
        {
            if (assessments.Count > 0)
            {
                assessments.Sort((a1, a2) => a1.DangerValue.CompareTo(a2.DangerValue));
                for (int i = 0; i < assessments.Count; i++)
                {
                    if (assessments[i].AdjacentCharacters.Count > 0)
                    {
                        for (int j = 0; j < assessments[i].AdjacentCharacters.Count; j++)
                        {
                            if (assessments[i].AdjacentCharacters[j].stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                            {
                                return assessments[i].Space;
                            }
                        }                        
                    }
                }
                return SafestNode;
            }
            else
            {
                return SafestNode;
            }
        }
    }
    public Node SafestLowThreatMelee
    {
        get
        {
            if (assessments.Count > 0)
            {
                if (evaluations.Count > 0)
                {
                    List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();
                    evaluations.Sort((e1, e2) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                    for (int i = 0; i < evaluations[0].Space.neighbors.Length; i++)
                    {
                        Node n = evaluations[0].Space.neighbors[i];
                        if (n != null)
                        {
                            if (AssessmentByNode(n) != null)
                                temp.Add(AssessmentByNode(n));
                        }
                    }
                    if (temp.Count > 0)
                    {
                        temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                        return temp[0].Space;
                    }
                }
                return SafestAnyMeleeNode;
            }
            else return SafestAnyMeleeNode;
        }
    }
    public Node SafestHighThreatMelee
    {
        get
        {
            if (assessments.Count > 0)
            {
                if (evaluations.Count > 0)
                {
                    List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();
                    evaluations.Sort((e2, e1) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                    for (int i = 0; i < evaluations[0].Space.neighbors.Length; i++)
                    {
                        Node n = evaluations[0].Space.neighbors[i];
                        if (n != null)
                        {
                            if (AssessmentByNode(n) != null)
                                temp.Add(AssessmentByNode(n));
                        }
                    }
                    if (temp.Count > 0)
                    {
                        temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                        return temp[0].Space;
                    }
                }
                return SafestAnyMeleeNode;
            }
            else return SafestAnyMeleeNode;
        }
    }
    public Node SafestHighThreatEfficientNode
    {
        get
        {
            if (assessments.Count > 0)
            {
                if (evaluations.Count > 0)
                {
                    List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();
                    evaluations.Sort((e2, e1) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                    EnemyEvaluation ee = null;
                    List<Action.Condition> c = new List<Action.Condition>();

                    // Look for vulnerable first
                    for (int i = 0; i < evaluations.Count; i++)
                    {
                        c.Clear();
                        c = evaluations[i].Enemy.Conditions;
                        if (c.Contains(Action.Condition.Vulnerable))
                        {
                            ee = evaluations[i];
                            break;
                        }
                    }

                    // If no vulnerable, look for non-armored
                    if (ee == null)
                    {
                        for (int i = 0; i < evaluations.Count; i++)
                        {
                            c.Clear();
                            c = evaluations[i].Enemy.Conditions;
                            if (!c.Contains(Action.Condition.Armor))
                            {
                                ee = evaluations[i];
                                break;
                            }
                        }
                    }
                    if (ee == null) return SafestHighThreatMelee;

                    for (int i = 0; i < ee.Space.neighbors.Length; i++)
                    {
                        Node n = ee.Space.neighbors[i];
                        if (n != null)
                        {
                            if (AssessmentByNode(n) != null)
                                temp.Add(AssessmentByNode(n));
                        }
                    }
                    if (temp.Count > 0)
                    {
                        temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                        return temp[0].Space;
                    }
                }
                return SafestAnyMeleeNode;
            }
            else return SafestAnyMeleeNode;
        }
    }
    public Node SafestLowThreatEfficientNode
    {
        get
        {
            if (assessments.Count > 0)
            {
                if (evaluations.Count > 0)
                {
                    List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();
                    evaluations.Sort((e1, e2) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                    EnemyEvaluation ee = null;
                    List<Action.Condition> c = new List<Action.Condition>();

                    // Look for vulnerable first
                    for (int i = 0; i < evaluations.Count; i++)
                    {
                        c.Clear();
                        c = evaluations[i].Enemy.Conditions;
                        if (c.Contains(Action.Condition.Vulnerable))
                        {
                            ee = evaluations[i];
                            break;
                        }
                    }

                    // If no vulnerable, look for non-armored
                    if (ee == null)
                    {
                        for (int i = 0; i < evaluations.Count; i++)
                        {
                            c.Clear();
                            c = evaluations[i].Enemy.Conditions;
                            if (!c.Contains(Action.Condition.Armor))
                            {
                                ee = evaluations[i];
                                break;
                            }
                        }
                    }
                    if (ee == null) return SafestHighThreatMelee;

                    for (int i = 0; i < ee.Space.neighbors.Length; i++)
                    {
                        Node n = ee.Space.neighbors[i];
                        if (n != null)
                        {
                            if (AssessmentByNode(n) != null)
                                temp.Add(AssessmentByNode(n));
                        }
                    }
                    if (temp.Count > 0)
                    {
                        temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                        return temp[0].Space;
                    }
                }
                return SafestAnyMeleeNode;
            }
            else return SafestAnyMeleeNode;
        }
    }
    public Node SafestMeleeWithConditionNode(Action.Condition condition, bool highThreat)
    {
        if (assessments.Count > 0)
        {
            if (evaluations.Count > 0)
            {
                List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();
                if (highThreat)
                    evaluations.Sort((e2, e1) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                else
                    evaluations.Sort((e1, e2) => e1.ThreatValue.CompareTo(e2.ThreatValue));
                EnemyEvaluation ee = null;
                List<Action.Condition> c = new List<Action.Condition>();

                for (int i = 0; i < evaluations.Count; i++)
                {
                    c.Clear();
                    c = evaluations[i].Enemy.Conditions;
                    if (c.Contains(condition))
                    {
                        ee = evaluations[i];
                        break;
                    }
                }

                for (int i = 0; i < ee.Space.neighbors.Length; i++)
                {
                    Node n = ee.Space.neighbors[i];
                    if (n != null)
                    {
                        if (AssessmentByNode(n) != null)
                            temp.Add(AssessmentByNode(n));
                    }
                }
                if (temp.Count > 0)
                {
                    temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                    return temp[0].Space;
                }
            }
            return null;
        }
        else return null;
    }
    public Node SafestMeleeWithSpecificCharacterNode(Character c)
    {
        if (assessments.Count > 0)
        {
            if (evaluations.Count > 0)
            {
                List<GridSpaceAssessment> temp = new List<GridSpaceAssessment>();

                EnemyEvaluation ee = EvaluationByCharacter(c);
                for (int i = 0; i < ee.Space.neighbors.Length; i++)
                {
                    Node n = ee.Space.neighbors[i];
                    if (n != null)
                    {
                        if (AssessmentByNode(n) != null)
                            temp.Add(AssessmentByNode(n));
                    }
                }
                if (temp.Count > 0)
                {
                    temp.Sort((t1, t2) => t1.DangerValue.CompareTo(t2.DangerValue));
                    return temp[0].Space;
                }
            }
        }
        return null;
    }

    public void UpdateEvaluationAndAssessment()
    {
        // Each character updates their own threat value when they act.
        // This method is called before the AI moves.
        assessments.Clear();
        foreach (Node node in CombatGrid.grid)
        {
            if (CombatGrid.NodeToDistance(character.movement.currentNode, node) <= character.movement.movementLeft)
            {
                assessments.Add(new GridSpaceAssessment(character, node, this));
            }
        }
    }
    private void Start()
    {
        memory = new LastRoundMemory();
    }

    public void Interaction(Interaction interaction)
    {
        // Update the memory here
    }

    private void Behavior()
    {
        // this needs to be checked before movement

        // Update the AI's knowledge before picking a destination
        UpdateEvaluationAndAssessment();

        // Then use that knowledge based on the AI's personality
        switch (character.stats.personality)
        {
            case CharacterStats.Personality.Aggressive:
                Aggressive();
                break;
            case CharacterStats.Personality.Annoying:
                Annoying();
                break;
            case CharacterStats.Personality.Cunning:
                Cunning();
                break;
            case CharacterStats.Personality.Efficient:
                Efficient();
                break;
            case CharacterStats.Personality.Fearful:
                Fearful();
                break;
            case CharacterStats.Personality.PainAverse:
                PainAverse();
                break;
            case CharacterStats.Personality.Reckless:
                Reckless();
                break;
            case CharacterStats.Personality.Relentless:
                Relentless();
                break;
        }
    }

    #region Behaviors
    private void Aggressive()
    {
        Destination = MostDangerousNode;
    }
    private void Annoying()
    {
        if(memory.DamageDealt > 0)
        {
            Destination = SafestNode;
        }
        else
        {
            Destination = SafestLowThreatEfficientNode;
        }
    }
    private void Cunning()
    {
        if (memory.Healers.Count > 0)
        {
            Destination = SafestMeleeWithSpecificCharacterNode(memory.Healers[0]);
        }
        else
        {
            Destination = SafestHighThreatEfficientNode;
        }
    }
    private void Efficient()
    {
        Destination = SafestLowThreatEfficientNode;
    }
    private void Fearful()
    {
        Destination = SafestNode;
    }
    private void PainAverse()
    {
        if (memory.DamageTaken > 0)
        {
            Destination = SafestNode;
        }
        else
        {
            Destination = SafestLowThreatMelee;
        }
    }
    private void Reckless()
    {
        Destination = SafestHighThreatEfficientNode;
    }
    private void Relentless()
    {
        Destination = SafestMeleeWithSpecificCharacterNode(relentlessTarget);
    }
    #endregion

    public GridSpaceAssessment AssessmentByNode(Node node)
    {
        GridSpaceAssessment output = null;
        for (int i = 0; i < assessments.Count; i++)
        {
            if (node == assessments[i].Space)
            {
                output = assessments[i];
                break;
            }
        }
        return output;
    }
    public EnemyEvaluation EvaluationByCharacter(Character c)
    {
        EnemyEvaluation ee = null;

        for (int i = 0; i < evaluations.Count; i++)
        {
            if (evaluations[i].Enemy == c)
            {
                ee = evaluations[i];
                break;
            }
        }

        return ee;
    }
}

public class GridSpaceAssessment
{
    private Node space;
    private Character origin;
    private MonsterAI ai;
    private float dangerValue;
    private List<Character> adjacentCharacters = new List<Character>();

    public GridSpaceAssessment(Character _origin, Node _space, MonsterAI _ai)
    {
        space = _space;
        origin = _origin;
        ai = _ai;
    }
    public Node Space
    {
        get { return space; }
    }
    public bool WithinReach
    {
        get
        {
            return CombatGrid.NodeToDistance(origin.movement.currentNode, space) <= origin.stats.movement;
        }
    }
    public List<Character> AdjacentCharacters
    {
        get { return adjacentCharacters; }
    }
    public float DangerValue
    {
        get
        {
            dangerValue = 0;
            for (int i = 0; i < CombatManager.actors.Count; i++)
            {
                Character actor = CombatManager.actors[i];
                if (actor.alive && actor != origin)
                {
                    dangerValue += DangerModifier(actor);
                }
            }
            return dangerValue;
        }
    }

    public float DangerModifier(Character character)
    {
        adjacentCharacters.Clear();
        float output = 0;
        bool isMerc = character.stats.characterType == CharacterStats.CharacterTypes.Adventurer;
        int distance = CombatGrid.NodeToDistance(space, character.movement.currentNode);
        float threatLevel = 0;
        if (isMerc)
        {            
            for (int i = 0; i < ai.evaluations.Count; i++)
            {
                if (ai.evaluations[i].Enemy == character) threatLevel = ai.evaluations[i].ThreatValue;
            }
        }

        if (distance == 1)
        {
            output = isMerc ? threatLevel+2 : -6;
            adjacentCharacters.Add(character);
        }
        else
        {
            output = isMerc ? (distance <= character.stats.movement + 1 ? threatLevel - distance - 1 : -1) : -5 + distance - 1;
        }
        return output;
    }
}

public class LastRoundMemory
{
    private float damageDealt = 0;
    private float damageTaken = 0;
    private float maxDamageTaken = 0;
    private List<Action.Condition> conditionsGained = new List<Action.Condition>();
    private List<Character.DamageTypes> damageTypesTaken = new List<Character.DamageTypes>();
    private List<Character> healers = new List<Character>();
    private List<Character> hurtMe = new List<Character>();
    private Character biggestThreat;

    public List<Action.Condition> ConditionsGained
    {
        get { return conditionsGained; }
    }
    public List<Character.DamageTypes> DamageTypesTaken
    {
        get { return damageTypesTaken; }
    }
    public float DamageDealt
    {
        get{ return damageDealt; }
        set { damageDealt = value; }
    }
    public float DamageTaken
    {
        get { return damageTaken; }
        set
        {
            if (value > maxDamageTaken)
            {
                maxDamageTaken = value;
                biggestThreat = hurtMe[hurtMe.Count - 1];
            }
            damageTaken += value;
        }
    }
    public Character BiggestThreat
    {
        get { return biggestThreat; }
    }
    public List<Character> Healers
    {
        get { return healers; }
    }
    public void GainedCondition(Action.Condition condition)
    {
        conditionsGained.Add(condition);
    }
    public void AddDamageType(Character.DamageTypes dtype)
    {
        damageTypesTaken.Add(dtype);
    }
    public void AddHealer(Character character)
    {
        healers.Add(character);
    }
    public void AddHurter(Character character)
    {
        hurtMe.Add(character);
    }

    public void ClearMemory()
    {
        hurtMe.Clear();
        healers.Clear();
        biggestThreat = null;
        damageTaken = 0;
        maxDamageTaken = 0;
        conditionsGained.Clear();
        damageTypesTaken.Clear();
    }
}
public class EnemyEvaluation
{
    private Character enemy;
    private float threatValue = 0;

    public EnemyEvaluation(Character _enemy)
    {
        enemy = _enemy;
    }

    public Character Enemy
    {
        get { return enemy; }
        set { enemy = value; }
    }
    public float ThreatValue
    {
        get {
            threatValue += ThreatGenerated;
            return threatValue; }
    }
    public float ThreatGenerated
    {
        get { return CombatManager.threat[enemy]; }
    }
    public Node Space
    {
        get
        {
            return enemy.movement.currentNode;
        }
    }    
}
