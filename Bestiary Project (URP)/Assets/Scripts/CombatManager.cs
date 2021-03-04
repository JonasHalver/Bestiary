using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public static CombatManager instance;
    public static List<Character> actors = new List<Character>();

    public Transform combatUI;

    public static List<CombatAction> combatActions = new List<CombatAction>();
    public static Dictionary<Character, Vector2> characterPositions = new Dictionary<Character, Vector2>();
    public event System.Action updateCombat;
    public event System.Action startCombat;
    public TextMeshProUGUI log;

    public bool combatStarted;

    public static event System.Action StartRound, EndRound;
    public static bool combatFlag = false;

    public enum CombatStage { Setup, Combat, EnemyMovement }
    public CombatStage currentStage = CombatStage.Setup;
    public TextMeshProUGUI stageDisplay;
    private bool interactableCheck = true;

    private void Awake()
    {
        instance = this;
        AdventurerMovement.newPosition += Delay;
    }

    private void Start()
    {
        GameManager.GamePaused += Paused;
    }

    private void Paused()
    {
        UIInteractable(!GameManager.paused, true);
    }

    private void Update()
    {
        if (!combatStarted)
        {
            bool flag = false;
            for (int i = 0; i < actors.Count; i++)
            {
                if (actors[i].movement.currentNode == null) flag = true;
            }
            if (flag)
            {
                startCombat.Invoke();
                actors.Sort((actor2, actor1) => actor1.stats.speed.CompareTo(actor2.stats.speed));
                combatStarted = true;
            }
        }

        if(combatActions.Count > 0) log.text = LogConstructor();

        switch (currentStage)
        {
            case CombatStage.Setup:
                stageDisplay.text = "Setup";
                break;
            case CombatStage.Combat:
                stageDisplay.text = "Combat";
                break;
            case CombatStage.EnemyMovement:
                stageDisplay.text = "Enemy Movement";
                break;
        }
    }

    public void UpdateCombat()
    {
        combatActions.Clear();
        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i].alive)
            {
                CombatAction newCombatAction = actors[i].CombatAction(new BattlefieldPositionInfo(actors[i], characterPositions));
                if (newCombatAction != null)
                {
                    if (actors[i].conditions.Contains(Debuff.ControlType.Blind))
                    {
                        if (actors[i].lastCombatAction == null)
                        {
                            combatActions.Add(newCombatAction);
                            actors[i].lastCombatAction = newCombatAction;
                        }
                        else
                        {
                            combatActions.Add(actors[i].lastCombatAction);
                        }
                    }
                    else
                    {
                        combatActions.Add(newCombatAction);
                    }
                }
            }
        }
        for (int i = 0; i < combatActions.Count; i++)
        {
            if (combatActions[i].action != null)
            {
                combatActions[i].affectedNodes = CombatGrid.NodesAffectedByAction(combatActions[i]);
                combatActions[i].origin.currentAction = combatActions[i];
            }
        }

        //CombatGrid.instance.HighlighAction(combatActions);
    }

    public void Commit()
    {
        UpdateCombat();
        UIInteractable(false, false);
        Delay("StartOfRound");
    }

    public string LogConstructor()
    {
        string log = "Combat Log:" + Environment.NewLine;
        string line = "";
        bool flag = false;
        for (int i = 0; i < actors.Count; i++)
        {
            flag = false;
            foreach (CombatAction ca in combatActions)
            {
                if (ca != null && ca.origin == actors[i])
                {
                    if (ca.highlighted) line = "<color=#FFC70B>"; else line = "<color=#FFFFFF>";
                    line += ca.origin.stats.characterName + " will use " + ca.action.actionName + ".</color>" + Environment.NewLine;
                    log += line;
                    flag = true;
                }                
            }
            if (!flag)
            {
                line += actors[i].stats.characterName + " has no good targets, and will pass." + Environment.NewLine;
                log += line;
            }
        }        

        return log;
    }

    IEnumerator CombatRound()
    {
        currentStage = CombatStage.Combat;
        for (int i = 0; i < actors.Count; i++)
        {
            CombatGrid.StopHighlight();
            if (!actors[i].alive) continue;
            CombatGrid.HighlightNodeStatic(actors[i].movement.currentNode);
            yield return new WaitForSecondsRealtime(0.5f);
            foreach (CombatAction a in combatActions) a.highlighted = false;
            foreach(CombatAction action in combatActions)
            {
                if (action.origin == actors[i] && action.origin.alive)
                {
                    action.highlighted = true;
                    Vector2 dir = (action.targetNode.coordinate - actors[i].position).normalized;
                    actors[i].StartCoroutine("TakeAction", dir);
                    while (!combatFlag)
                    {
                        yield return null;
                    }
                    action.ResolveAction();
                }
            }
            combatFlag = false;
            yield return new WaitForSecondsRealtime(1);
        }
        StartCoroutine(EnemyMovement());
    }

    public void StartOfRound()
    {
        for(int i = 0; i < actors.Count; i++)
        {
            actors[i].ResetStats();
            for (int b = 0; b < actors[i].buffs.Count; b++)
            {
                actors[i].buffs[b].ResolveBuff();
            }
            for (int d = 0; d < actors[i].debuffs.Count; d++)
            {              
                actors[i].debuffs[d].ResolveDebuff();
            }
            actors[i].UpdateBuffsAndDebuffs();
        }
        //StartRound.Invoke();

        Delay("CombatRound");
    }

    IEnumerator EnemyMovement()
    {
        currentStage = CombatStage.EnemyMovement;
        yield return null;
        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i].stats.characterType == CharacterStats.CharacterTypes.NPC && actors[i].alive)
            {
                actors[i].movement.AIMovement();
            }
            yield return new WaitForSecondsRealtime(0.5f);
        }
        EndRound.Invoke();
        Delay("UpdateCombat");
        UIInteractable(true, false);
        currentStage = CombatStage.Setup;

    }

    public void UIInteractable(bool check, bool fromPause)
    {
        if (!fromPause) interactableCheck = check;
        foreach(Transform item in combatUI)
        {
            Button b = item.GetComponent<Button>();
            if (b)
            {
                // this whole thing might be a mess
                if (fromPause && check && interactableCheck) b.interactable = true;
                else if (fromPause && check && !interactableCheck) b.interactable = false;
                else if (fromPause && !check) b.interactable = false;
                else if (!fromPause && !GameManager.paused) b.interactable = check;
                else if (!fromPause && GameManager.paused) b.interactable = false;
            }
        }
    }

    public void Delay()
    {
        StartCoroutine(OneFrameDelay("UpdateCombat"));
    }

    public void Delay(string message)
    {
        StartCoroutine(OneFrameDelay(message));
    }

    IEnumerator OneFrameDelay(string message)
    {
        yield return null;
        SendMessage(message);
    }
}

public class CombatAction
{
    public Character origin;
    public Character target;
    public Node targetNode;
    public Action action;
    public bool valid;
    public bool highlighted = false;

    public List<Node> affectedNodes = new List<Node>();

    public BattlefieldPositionInfo bpi;

    public CombatAction(Character _origin, Node _targetNode, Action _action)
    {
        origin = _origin;
        targetNode = _targetNode;
        action = _action;

        if (targetNode != null && targetNode.occupant != null) target = targetNode.occupant;
    }

    public void ResolveAction()
    { 
        foreach (Node node in affectedNodes)
        {
            node.ActionHighlight();
            if (node.occupant != null)
            {
                Character actionTarget = node.occupant;
                if (actionTarget != null && !actionTarget.alive) return;
                bool targetIsAlly = actionTarget != null ? (Character.AllyOrEnemy(origin, actionTarget) || actionTarget == origin) : false;
                bool targetIsSelf = actionTarget != null ? actionTarget == origin : false;
                Interaction newInteraction = null;
                switch (action.actionType)
                {
                    case Action.ActionType.Attack:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.value);
                        else if (targetIsAlly && (action.canFriendlyFire || action.targetGroup == Action.TargetGroup.All)) newInteraction = new Interaction(origin, node, action, action.value);
                        else if (!targetIsAlly) newInteraction = new Interaction(origin, node, action, action.value);
                        break;
                    case Action.ActionType.Healing:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.value);
                        else if (targetIsAlly) newInteraction = new Interaction(origin, node, action, action.value);
                        break;
                    case Action.ActionType.Buff:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.buff);
                        else if (targetIsAlly) newInteraction = new Interaction(origin, node, action, action.buff);
                        break;
                    case Action.ActionType.Debuff:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.value, action.debuff);
                        else if (!targetIsAlly) newInteraction = new Interaction(origin, node, action, action.debuff);
                        break;
                    case Action.ActionType.AttackDebuff:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.value, action.debuff);
                        else if (!targetIsAlly) newInteraction = new Interaction(origin, node, action, action.debuff);

                        break;
                    case Action.ActionType.HealingBuff:
                        if (targetIsSelf && action.canHitSelf) newInteraction = new Interaction(origin, node, action, action.buff);
                        else if (targetIsAlly) newInteraction = new Interaction(origin, node, action, action.buff);
                        break;
                }

                if (newInteraction != null) actionTarget.Interaction(newInteraction);
            }
        }

        origin.lastCombatAction = null;
    }
}

public class BattlefieldPositionInfo
{
    public Character origin;

    public List<Character> alliesAlive = new List<Character>();
    public List<Character> alliesDead = new List<Character>();

    public List<Character> enemiesAlive = new List<Character>();
    public List<Character> enemiesDead = new List<Character>();

    public int distanceToClosestEnemy;
    public Character closestEnemy;
    public Character farthestEnemy;
    public int distanceToClosestAlly;
    public Character closestAlly;
    public Character farthestAlly;

    public Character lowestHealthEnemy;
    public Character lowestHealthAlly;

    public List<Character> clumpedEnemies = new List<Character>();
    public List<Character> clumpedAllies = new List<Character>();

    public Dictionary<Character, int> allyDistances = new Dictionary<Character, int>();
    public Dictionary<Character, int> enemyDistances = new Dictionary<Character, int>();

    public List<Character> enemiesInMelee = new List<Character>();
    public List<Character> alliesInMelee = new List<Character>();

    public Character enemyWithMostNeighbors;
    public Character allyWithMostNeighbors;

    bool flag1, flag2, flag3, flag4, flag5, flag6;

    public BattlefieldPositionInfo (Character actor, Dictionary<Character, Vector2> kvp)
    {
        origin = actor;
        FillOutInfo(actor, kvp);

        foreach (KeyValuePair<Character, int> positions in allyDistances)
        {
            if (positions.Value == 1) alliesInMelee.Add(positions.Key);
        }
        foreach(KeyValuePair<Character, int> positions in enemyDistances)
        {            
            if (positions.Value == 1) enemiesInMelee.Add(positions.Key);
        }

        //FindClumpedAllies(actor, kvp);
        //FindClumpedEnemies(actor, kvp);
        //EnemyMostNeighbors(actor, kvp);
        //AllyMostNeighbors(actor, kvp);
        //FindClosestAndFarthest(actor, kvp);

        //if (!flag1 || !flag2 || !flag3 || !flag4 || !flag5 || !flag6)
        //{
        //    Debug.LogError("BPI failed, 1 = " + flag1 + ", 2 = " + flag2 + ", 3 = " + flag3 + ", 4 = " + flag4 + ", 5 = " + flag5 + ", 6 = " + flag6);
        //}
    }

    public void FillOutInfo(Character actor, Dictionary<Character, Vector2> kvp)
    {
        float allyHealth = 1000;
        float enemyHealth = 1000;


        foreach (KeyValuePair<Character, Vector2> character in kvp)
        {
            Character c = character.Key;
            if (actor == c || c == null || c.stats == null || actor == null || actor.stats == null)
            {
            }
            else
            {
                if (c.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                {
                    if (c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer) alliesAlive.Add(c);
                    else if (c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.NPC) enemiesAlive.Add(c);
                    else if (!c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer) alliesDead.Add(c);
                    else if (!c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.NPC) enemiesDead.Add(c);

                    if (actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer && c.alive) allyDistances.Add(c, CombatGrid.Vector2ToDistance(kvp[actor], character.Value));
                    else if (actor.stats.characterType == CharacterStats.CharacterTypes.NPC && c.alive) enemyDistances.Add(c, CombatGrid.Vector2ToDistance(kvp[actor], character.Value));

                    if (actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer)
                    {
                        if (allyHealth > c.currentHitpoints && c.alive)
                        {
                            lowestHealthAlly = c;
                            allyHealth = c.currentHitpoints;
                        }
                    }
                    else
                    {
                        if (enemyHealth > c.currentHitpoints && c.alive)
                        {
                            lowestHealthEnemy = c;
                            enemyHealth = c.currentHitpoints;
                        }
                    }
                }
                else
                {
                    if (c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer) enemiesAlive.Add(c);
                    else if (c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.NPC) alliesAlive.Add(c);
                    else if (!c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer) enemiesDead.Add(c);
                    else if (!c.alive && actor.stats.characterType == CharacterStats.CharacterTypes.NPC) alliesDead.Add(c);

                    if (actor.stats.characterType == CharacterStats.CharacterTypes.Adventurer && c.alive) enemyDistances.Add(c, CombatGrid.Vector2ToDistance(kvp[actor], character.Value));
                    else if (actor.stats.characterType == CharacterStats.CharacterTypes.NPC && c.alive) allyDistances.Add(c, CombatGrid.Vector2ToDistance(kvp[actor], character.Value));

                    if (actor.stats.characterType == CharacterStats.CharacterTypes.NPC)
                    {
                        if (allyHealth > c.currentHitpoints && c.alive)
                        {
                            lowestHealthAlly = c;
                            allyHealth = c.currentHitpoints;
                        }
                    }
                    else
                    {
                        if (enemyHealth > c.currentHitpoints && c.alive)
                        {
                            lowestHealthEnemy = c;
                            enemyHealth = c.currentHitpoints;
                        }
                    }
                }
            }
        }
        flag1 = true;
    }

    public void FindClumpedEnemies(Character actor, Dictionary<Character, Vector2> enemies)
    {
        foreach(KeyValuePair<Character, Vector2> enemy in enemies)
        {
            Character e = enemy.Key;
            if (e.stats.characterType != actor.stats.characterType)
            {
                foreach (KeyValuePair<Character, Vector2> enemy2 in enemies)
                {
                    Character e2 = enemy2.Key;
                    // if (e == e2 || !e.alive || !e2.alive) return;

                    if (e != e2 && e2.stats.characterType != actor.stats.characterType)
                    {
                        if (CombatGrid.Vector2ToDistance(enemy.Value, enemy2.Value) == 1)
                        {
                            if (!clumpedEnemies.Contains(e)) clumpedEnemies.Add(e);
                            if (!clumpedEnemies.Contains(e2)) clumpedEnemies.Add(e2);
                        }
                    }
                }
            }
        }
        flag2 = true;
    }

    public void EnemyMostNeighbors(Character actor, Dictionary<Character, Vector2> enemies)
    {
        if (clumpedEnemies.Count > 2)
        {
            Character output = null;
            foreach(KeyValuePair<Character, Vector2> enemy in enemies)
            {
                int count = 0;
                if (enemy.Key.stats.characterType == actor.stats.characterType) return;
                foreach (KeyValuePair<Character, Vector2> enemy2 in enemies)
                {
                    if (enemy2.Key.stats.characterType == actor.stats.characterType || enemy2.Key == enemy.Key) return;
                    if (CombatGrid.Vector2ToDistance(enemy.Value, enemy2.Value) == 1)
                    {
                        count++;
                    }
                }
                if (count >= 2)
                {
                    output = enemy.Key;
                    break;
                }
            }
            if (output == null) enemyWithMostNeighbors = clumpedEnemies[UnityEngine.Random.Range(0, clumpedEnemies.Count)];
            else enemyWithMostNeighbors = output;
        }
        else if (clumpedEnemies.Count > 0 && clumpedEnemies.Count <= 2)
        {
            enemyWithMostNeighbors = clumpedEnemies[UnityEngine.Random.Range(0,2)];
        }
        flag3 = true;
    }

    public void FindClumpedAllies(Character actor, Dictionary<Character, Vector2> allies)
    {
        foreach (KeyValuePair<Character, Vector2> ally in allies)
        {
            Character a = ally.Key;

            if (a.stats.characterType == actor.stats.characterType)
            {
                foreach (KeyValuePair<Character, Vector2> ally2 in allies)
                {
                    Character a2 = ally2.Key;
                    //if (a == a2 || !a.alive || !a2.alive) return;
                    if (a != a2 && a2.stats.characterType == actor.stats.characterType)
                    {
                        if (CombatGrid.Vector2ToDistance(ally.Value, ally2.Value) == 1)
                        {
                            if (!clumpedEnemies.Contains(a)) clumpedEnemies.Add(a);
                            if (!clumpedEnemies.Contains(a2)) clumpedEnemies.Add(a2);
                        }
                    }
                }
            }
        }
        flag4 = true;
    }
    public void AllyMostNeighbors(Character actor, Dictionary<Character, Vector2> allies)
    {
        if (clumpedAllies.Count > 2)
        {
            Character output = null;
            foreach (KeyValuePair<Character, Vector2> ally in allies)
            {
                int count = 0;
                if (ally.Key.stats.characterType != actor.stats.characterType) return;
                foreach (KeyValuePair<Character, Vector2> ally2 in allies)
                {
                    if (ally2.Key.stats.characterType != actor.stats.characterType || ally2.Key == ally.Key) return;
                    if (CombatGrid.Vector2ToDistance(ally.Value, ally2.Value) == 1)
                    {
                        count++;
                    }
                }
                if (count >= 2)
                {
                    output = ally.Key;
                    break;
                }
            }
            if (output == null) allyWithMostNeighbors = clumpedAllies[UnityEngine.Random.Range(0, clumpedAllies.Count)];
            else allyWithMostNeighbors = output;
        }
        else if (clumpedAllies.Count > 0 && clumpedAllies.Count <= 2)
        {
            allyWithMostNeighbors = clumpedAllies[UnityEngine.Random.Range(0, 2)];
        }
        flag5 = true;
    }

    public void FindClosestAndFarthest(Character actor, Dictionary<Character, Vector2> kvp)
    {
        int enemyCloseDist = 1000, allyCloseDist = 1000;
        int enemyFarDist = 0, allyFarDist = 0;
        foreach (KeyValuePair<Character, Vector2> character in kvp)
        {
            if (alliesAlive.Contains(character.Key))
            {
                if (CombatGrid.Vector2ToDistance(actor.position, character.Value) < allyCloseDist)
                {
                    closestAlly = character.Key;
                    allyCloseDist = CombatGrid.Vector2ToDistance(actor.position, character.Value);
                }
                if (CombatGrid.Vector2ToDistance(actor.position, character.Value) > allyFarDist)
                {
                    farthestAlly = character.Key;
                    allyFarDist = CombatGrid.Vector2ToDistance(actor.position, character.Value);
                }
            }
            if (enemiesAlive.Contains(character.Key))
            {
                if (CombatGrid.Vector2ToDistance(actor.position, character.Value) < enemyCloseDist)
                {
                    closestEnemy = character.Key;
                    enemyCloseDist = CombatGrid.Vector2ToDistance(actor.position, character.Value);
                }
                if (CombatGrid.Vector2ToDistance(actor.position, character.Value) > enemyFarDist)
                {
                    farthestEnemy = character.Key;
                    enemyFarDist = CombatGrid.Vector2ToDistance(actor.position, character.Value);
                }
            }
        }
        flag6 = true;
    }
}

public class Interaction
{
    // For interacting with nodes in combat

    public Node affectedNode;
    public Character origin;
    public Action action;
    public float value = 1;
    public bool applyBuff;
    public bool applyDebuff;

    public Buff buff;
    public Debuff debuff;

    public Interaction(Character _origin, Node _affectedNode, Action _action)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
    }
    public Interaction(Character _origin, Node _affectedNode, Action _action, Debuff _debuff)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
        applyDebuff = true;
        debuff = _debuff;
    }
    public Interaction(Character _origin, Node _affectedNode, Action _action, float _value, Debuff _debuff)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
        applyDebuff = true;
        debuff = _debuff;
        value = _value;
    }

    public Interaction(Character _origin, Node _affectedNode, Action _action, Buff _buff)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
        applyBuff = true;
        buff = _buff;
    }
    public Interaction(Character _origin, Node _affectedNode, Action _action, float _value, Buff _buff)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
        applyBuff = true;
        buff = _buff;
        value = _value;
    }
    public Interaction(Character _origin, Node _affectedNode, Action _action, float _value)
    {
        affectedNode = _affectedNode;
        origin = _origin;
        action = _action;
        value = _value;
    }
}