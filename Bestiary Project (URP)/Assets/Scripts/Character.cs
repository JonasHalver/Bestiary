using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ConditionManager))]
public class Character : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum DamageTypes { Cutting, Piercing, Crushing, Fire, Acid, Cold, Poison, Healing, None }
    public CharacterStats stats;
    public float damageTaken = 0;
    public float currentHitpoints;
    public bool alive = true;
    public AdventurerMovement movement;
    public Vector2 position;
    public List<Debuff> debuffs = new List<Debuff>();
    public List<Buff> buffs = new List<Buff>();
    public CombatAction currentAction;

    public Image characterIcon;
    public Color characterIconColor = Color.white;
    public Image deadImage;

    public event System.Action<Debuff> AcquiredDebuff;
    public event System.Action<Debuff> LostDebuff;

    public event System.Action<Buff> AcquiredBuff;
    public event System.Action<Buff> LostBuff;

    public MonsterAI AI
    {
        get;set;
    }

    //public List<Debuff.ControlType> conditions = new List<Debuff.ControlType>();
    public Dictionary<Action.Condition, int> Conditions
    {
        get
        {
            return conditions.Conditions;
        }
    }
    public List<Buff.BuffType> currentBuffs = new List<Buff.BuffType>();

    [HideInInspector]
    public CombatAction lastCombatAction;
    [HideInInspector]
    public List<Debuff> expiredDebuffs = new List<Debuff>();
    [HideInInspector]
    public List<Buff> expiredBuffs = new List<Buff>();
    [HideInInspector]
    public List<DamageTypes> temporaryResistances = new List<DamageTypes>();
    public int initiative;

    private Animator anim;
    private float t;
    public AnimationCurve attackAnimation;

    public event System.Action TookDamage;
    public event System.Action Healed;

    public int debuffCount, buffCount;

    public LastRoundMemory memory;
    [HideInInspector] public Entry entry;
    [HideInInspector] public bool highlightMyNode = false;
    public Action pass;
    public ConditionManager conditions;

    public Dictionary<Action, int> actionCooldowns = new Dictionary<Action, int>();

    private void Awake()
    {
        if (stats == null) CreateEmptyStats();
    }
    public void CreateEmptyStats()
    {
        CharacterStats newStats = ScriptableObject.CreateInstance<CharacterStats>();
        stats = newStats;
        Action newAction = ScriptableObject.CreateInstance<Action>();
        //newAction.actor = this;
        stats.actions.Add(newAction); stats.actions.Add(newAction); stats.actions.Add(newAction); stats.actions.Add(newAction);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        //CombatManager.StartOfTurn += StartOfTurn;
        //CombatManager.EndOfTurn += EndOfTurn;
        //CombatManager.EndOfMovement += EndOfMovement;
        CombatManager.RoundPhases += RoundChange;
        CombatManager.TurnPhases += OnMyTurn;
    }
    private void OnDisable()
    {
        CombatManager.RoundPhases -= RoundChange;
        CombatManager.TurnPhases -= OnMyTurn;
    }

    // Update is called once per frame
    void Update()
    {
        currentHitpoints = stats.hitPoints - damageTaken;
        currentHitpoints = Mathf.Clamp(currentHitpoints, 0, stats.hitPoints);

        if (currentHitpoints == 0) alive = false;
        if (!alive) deadImage.enabled = true;
        UpdatePosition();
        int initiativeMod = 0 - ((Conditions.ContainsKey(Action.Condition.SlowMonster) || Conditions.ContainsKey(Action.Condition.SlowMerc)) ? 2 : 0) + (Conditions.ContainsKey(Action.Condition.Haste) ? 2 : 0);
        initiative =  stats.speed + initiativeMod;

        buffCount = buffs.Count;
        debuffCount = debuffs.Count;

        damageTaken = Mathf.Clamp(damageTaken, 0, Mathf.Infinity);

        //if (stats.characterType == CharacterStats.CharacterTypes.NPC) 
            entry = stats.entry;


    }

    public void Created()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        movement.character = this;

        if (!CombatManager.actors.Contains(this)) CombatManager.actors.Add(this);
        stats.actions.Sort((action1, action2) => action1.actionPriority.CompareTo(action2.actionPriority));
        
        currentHitpoints = stats.hitPoints;
        initiative = stats.speed;
        for (int i = 0; i < stats.actions.Count; i++)
        {
            actionCooldowns.Add(stats.actions[i], 0);
            if (stats.actions[i].descriptionIndex > -1) stats.actions[i].actionDescription = Book.instance.descriptionsList.descriptions[i];
            stats.actions[i].Actor = this;
        }
        memory = new LastRoundMemory();
        characterIcon.sprite = stats.characterIcon;
        characterIcon.color = stats.characterIconColor;
        if(stats.characterType == CharacterStats.CharacterTypes.NPC)
        {
            AI = gameObject.AddComponent<MonsterAI>();
            AI.character = this;
        }
        anim.SetTrigger("FadeIn");
        movement.MoveByAction(movement.currentNode, true);
    }

    void UpdatePosition()
    {
        if (movement.currentNode != null)
        {
            position = movement.selected ? (movement.targetNode != null ? movement.targetNode.coordinate : movement.currentNode.coordinate) : movement.currentNode.coordinate;
            if (!CombatManager.characterPositions.ContainsKey(this)) CombatManager.characterPositions.Add(this, position);
            else CombatManager.characterPositions[this] = position;
        }

    }

    private void RoundChange(CombatManager.CombatTiming timing)
    {
        conditions.UpdateDurations(timing);
    }

    private void OnMyTurn(CombatManager.CombatTiming timing, Character currentActor)
    {
        if (currentActor != this) return;
        if (timing == CombatManager.CombatTiming.StartOfCharacterTurn) 
        { 
            conditions.TriggerOverTimeEffects();
            CooldownTick();            
        }
        conditions.UpdateDurations(timing);
    }
    public void CooldownTick()
    {
        int cooldownTimer;
        for (int i = 0; i < stats.actions.Count; i++)
        {
            cooldownTimer = actionCooldowns[stats.actions[i]];
            cooldownTimer--;
            cooldownTimer = Mathf.Max(0, cooldownTimer);
            actionCooldowns[stats.actions[i]] = cooldownTimer;
        }
    }

    #region Outdated Condition Code
    /*
    public void ResetStats()
    {
        currentBuffs.Clear();
        temporaryResistances.Clear();
        //conditions.Clear();
    }

    public void StartOfTurn(Character actor)
    {
        if (actor != this)
            return;
        StartCoroutine(UpdateDurations(Debuff.EffectTiming.StartOfTurn));
    }
    public void EndOfTurn(Character actor)
    {
        if (actor != this)
            return;
        StartCoroutine(UpdateDurations(Debuff.EffectTiming.EndOfTurn));

    }
    public void EndOfMovement(Character actor)
    {
        if (actor != this)
            return;
        StartCoroutine(UpdateDurations(Debuff.EffectTiming.Movement));
    }

    IEnumerator UpdateDurations(Debuff.EffectTiming timing)
    {
        foreach (Buff b in buffs) b.CheckDuration(timing);
        foreach (Debuff d in debuffs) d.CheckDuration(timing);
        yield return null;
        RemoveExpired();
    }

    public void RemoveExpired()
    {
        for (int i = 0; i < expiredBuffs.Count; i++)
        {
            buffs.Remove(expiredBuffs[i]);
            LostBuff.Invoke(expiredBuffs[i]);
        }
        for (int i = 0; i < expiredDebuffs.Count; i++)
        {
            expiredDebuffs[i].RemoveDebuff();
            debuffs.Remove(expiredDebuffs[i]);
            LostDebuff.Invoke(expiredDebuffs[i]);
        }
    }
    */
    #endregion

    public void Interaction(Interaction interaction)
    {
        #region Old Code
        /* Outdated
                   
        Debuff debuff = null; 
        Buff buff = null;
        bool flag = false;
        switch (interaction.action.actionType)
        {
            case Action.ActionType.Attack:
                if (!currentBuffs.Contains(Buff.BuffType.Dodge))
                    ReceiveHit(interaction.action, interaction.action.isCritical);
                break;
            case Action.ActionType.AttackDebuff:
                if (!currentBuffs.Contains(Buff.BuffType.Dodge))
                {

                    ReceiveHit(interaction.action, interaction.action.isCritical);
                    debuff = ScriptableObject.CreateInstance<Debuff>();
                    debuff.SetValues(interaction.debuff, interaction.action.value, this);
                    flag = false;
                    foreach (Debuff d in debuffs)
                    {
                        if (d.debuffType == debuff.debuffType)
                        {
                            if (d.debuffType == Debuff.DebuffType.Control && d.controlType == debuff.controlType)
                            {
                                d.durationRemaining += interaction.action.value;
                                flag = true;
                            }
                            else if (d.debuffType == Debuff.DebuffType.DamageOverTime && d.damageType == debuff.damageType)
                            {
                                d.durationRemaining += interaction.action.value;
                                flag = true;
                            }
                        }
                    }
                    if (!flag)
                    {
                        debuffs.Add(debuff);
                        debuff.DebuffApplied();
                        AcquiredDebuff.Invoke(debuff);
                    }
                }
                break;
            case Action.ActionType.Healing:
                ReceiveHealing(interaction.action);
                break;
            case Action.ActionType.HealingBuff:
                ReceiveHealing(interaction.action);
                buff = ScriptableObject.CreateInstance<Buff>();
                buff.SetValues(interaction.buff);
                buff.durationRemaining += interaction.action.value;
                buff.affectedCharacter = this;
                flag = false;
                foreach (Buff d in buffs)
                {
                    if (d.buffType == buff.buffType)
                    {
                        d.durationRemaining += interaction.action.value;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    buffs.Add(buff);
                    buff.ApplyBuff(true);
                    AcquiredBuff.Invoke(buff);
                }
                break;
            case Action.ActionType.Buff:

                buff = ScriptableObject.CreateInstance<Buff>();
                buff.SetValues(interaction.buff);
                buff.durationRemaining += interaction.action.value;
                buff.affectedCharacter = this;
                flag = false;
                foreach (Buff d in buffs)
                {
                    if (d.buffType == buff.buffType)
                    {
                        d.durationRemaining += interaction.action.value;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    buffs.Add(buff);
                    buff.ApplyBuff(true);
                    AcquiredBuff.Invoke(buff);
                }
                break;
            case Action.ActionType.Debuff:
                if (currentBuffs.Contains(Buff.BuffType.Dodge)) return;
                debuff = ScriptableObject.CreateInstance<Debuff>();
                debuff.SetValues(interaction.debuff, interaction.action.value, this);
                bool flag1 = false;
                foreach (Debuff d in debuffs)
                {
                    if (d.debuffType == debuff.debuffType)
                    {
                        if (d.debuffType == Debuff.DebuffType.Control && d.controlType == debuff.controlType)
                        {
                            d.durationRemaining += interaction.action.value;
                            flag1 = true;
                        }
                        else if (d.debuffType == Debuff.DebuffType.DamageOverTime && d.damageType == debuff.damageType)
                        {
                            d.durationRemaining += interaction.action.value;
                            flag1 = true;
                        }
                    }                    
                }
                if (!flag1)
                {
                    debuffs.Add(debuff);
                    debuff.DebuffApplied();
                    AcquiredDebuff.Invoke(debuff);
                }
                break;
        }
        */
        #endregion
        switch (interaction.effect.affectedGroup)
        {
            case Action.TargetGroup.All:
                break;
            case Action.TargetGroup.Allies:
                if (!AllyOrEnemy(this, interaction.origin)) return;
                break;
            case Action.TargetGroup.Enemies:
                if (AllyOrEnemy(this, interaction.origin)) return;
                break;
        }
        switch (interaction.effect.output)
        {
            case Action.Output.Damage:
                if (Conditions.ContainsKey(Action.Condition.Dodge)) break;
                ReceiveHit(interaction.effect);
                break;
            case Action.Output.Healing:
                ReceiveHealing(interaction.effect);
                break;
            case Action.Output.Condition:
                ReceiveCondition(interaction.effect);
                break;
            case Action.Output.Movement:
                MoveByAction(interaction);
                break;
        }
    }

    IEnumerator TakeAction(Vector2 dir)
    {
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            if (t >= 0.1f) CombatManager.combatFlag = true;
            anim.SetFloat("X", attackAnimation.Evaluate(t) * dir.x);
            anim.SetFloat("Y", attackAnimation.Evaluate(t) * dir.y);
            yield return null;
        }
    }
    
    public void ReceiveHit(OutputInfo info)
    {
        float damage = 1;
        if (info.critical || stats.weaknesses.Contains(info.damageType)) damage++;
        if (Conditions.ContainsKey(Action.Condition.Vulnerable)) damage++;
        if (stats.armored || currentBuffs.Contains(Buff.BuffType.Armor)) damage /= 2;
        if (stats.resistances.Contains(info.damageType)) damage /= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
        damageTaken += damage;
        TookDamage.Invoke();
    }

    public void ReceiveHit(DamageTypes conditionDamageType)
    {
        float damage = .5f;
        if (stats.resistances.Contains(conditionDamageType)) damage /= 2;
        if (stats.weaknesses.Contains(conditionDamageType)) damage *= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
        damageTaken += damage;
        TookDamage.Invoke();
    }

    public void ReceiveHealing(OutputInfo info)
    {
        float healing = info.value;

        currentHitpoints += healing;
        damageTaken -= healing;
        Healed.Invoke();
    }
    public void ReceiveHealing(bool fromRegeneration)
    {
        float healing = .5f;

        currentHitpoints += healing;
        damageTaken -= healing;
        Healed.Invoke();
    }
    public void ReceiveCondition(OutputInfo info)
    {
        conditions.ApplyCondition(info.condition, info.value);
    }

    public void MoveByAction(Interaction interaction)
    {
        Node target;
        if (interaction.primaryTarget.Character == this)
        {
            target = interaction.origin.movement.currentNode;
        }
        else
        {
            target = interaction.primaryTarget.Character.movement.currentNode;
        }
        Vector2 dir = (target.coordinate - movement.currentNode.coordinate).normalized;
        dir = new Vector2(Mathf.Round(dir.x), Mathf.Round(dir.y)) * (interaction.effect.towards ? 1 : -1);
        Vector2 move = Vector2.zero, destination = Vector2.zero;
        Node checkNode = movement.currentNode, destinationNode = movement.currentNode;
        Node lastValidNode = movement.currentNode;
        bool impactWall = false;
        bool impactCharacter = false;
        /*
        for (int i = interaction.effect.value; i > -1; i--)
        {
            move = new Vector2(Mathf.Round(dir.x * i), Mathf.Round(dir.y * i));
            destination = movement.currentNode.coordinate + move;
            // Check the path
            if (destination.x < 5 && destination.x > -1)
            {
                if (destination.y < 5 && destination.y > -1)
                {
                    checkNode = CombatGrid.grid[(int)destination.x, (int)destination.y];
                    if (checkNode.occupant != null && checkNode.occupant != interaction.origin)
                    {                        
                        if (checkNode.occupant == this)
                        {
                            break;
                        }
                        if (checkNode.occupant.CanBeMovedToNode(CombatGrid.NodeFromPosition(checkNode.coordinate + dir)))
                        {
                            impactCharacter = true;
                            lastValidNode = checkNode;
                        }
                    }
                    else if (checkNode.occupant != null && checkNode.occupant == interaction.origin)
                    {

                    }
                    else
                    {
                        if (!impactCharacter)
                            lastValidNode = checkNode;
                    }
                }
                else impactWall = true;
            }
            else impactWall = true;
        }
        */
        for (int i = 1; i < interaction.effect.value+1; i++)
        {
            move = new Vector2(Mathf.Round(dir.x * i), Mathf.Round(dir.y * i));
            destination = movement.currentNode.coordinate + move;
            // Check the path
            if (destination.x < 5 && destination.x > -1)
            {
                if (destination.y < 5 && destination.y > -1)
                {
                    checkNode = CombatGrid.grid[(int)destination.x, (int)destination.y];
                    if (checkNode.occupant != null && checkNode.occupant != interaction.origin && !AllyOrEnemy(checkNode.occupant, this))
                    {
                        if (checkNode.occupant == this)
                        {
                            break;
                        }
                        if (checkNode.occupant.CanBeMovedToNode(CombatGrid.NodeFromPosition(checkNode.coordinate + dir)))
                        {
                            impactCharacter = true;
                            lastValidNode = checkNode;
                            break;
                        }
                    }
                    else if (checkNode.occupant != null && checkNode.occupant == interaction.origin)
                    {

                    }
                    else
                    {
                        if (!impactCharacter)
                            lastValidNode = checkNode;
                    }
                }
                else impactWall = true;
            }
            else impactWall = true;
        }

        if (impactCharacter)
        {
            MoveByCollision(lastValidNode);
        }
        else
        {
            movement.MoveByAction(lastValidNode, false);

            if (impactWall)
            {
                ReceiveHit(DamageTypes.Crushing);
            }
        }
    }
    public bool CanBeMovedToNode(Node destination)
    {
        bool flag = false;
        if (destination == null) return false;
        Vector2Int dirToNode;
        dirToNode = Vector2Int.RoundToInt((destination.coordinate - position).normalized);

        if (destination.occupant != null)
        {
            Vector2Int newCoord = Vector2Int.RoundToInt(destination.coordinate) + dirToNode;
            if (CombatGrid.NodeIsOnGrid(newCoord))
            {
                if (destination.occupant.CanBeMovedToNode(CombatGrid.NodeFromPosition(newCoord)))
                {
                    flag = true;
                }
            }
        }
        else flag = true;

        return flag;
    }
    public void MoveByCollision(Node destination)
    {
        Vector2Int dirToNode;
        dirToNode = Vector2Int.RoundToInt((destination.coordinate - position).normalized);
        Vector2Int newCoord = Vector2Int.RoundToInt(destination.coordinate) + dirToNode;
        if (destination.occupant != null)
        {                        
            if (destination.occupant.CanBeMovedToNode(CombatGrid.NodeFromPosition(newCoord)))
            {
                movement.MoveByAction(destination, false);
                destination.occupant.MoveByCollision(CombatGrid.NodeFromPosition(newCoord));
            }
            else
            {
                destination.occupant.ReceiveHit(DamageTypes.Crushing);
                movement.MoveByAction(CombatGrid.NodeFromPosition(destination.coordinate - dirToNode), false);
            }
            ReceiveHit(DamageTypes.Crushing);
        }
        else
        {
            movement.MoveByAction(destination, false);
        }
        
    }
    

    public Action CurrentAction()
    {
        Action output = null;



        return output;
    }

    public static bool AllyOrEnemy(Character origin, Character target)
    {
        bool output;
        if (origin.stats.characterType == CharacterStats.CharacterTypes.Adventurer && target.stats.characterType == CharacterStats.CharacterTypes.Adventurer) output = true;
        else if (origin.stats.characterType == CharacterStats.CharacterTypes.NPC && target.stats.characterType == CharacterStats.CharacterTypes.NPC) output = true;
        else output = false;
        return output;
    }

    public CombatAction CombatAction(BattlefieldPositionInfo bpi)
    {
        List<CombatAction> availableActions = new List<CombatAction>();
        CombatAction output;

        for (int i = 0; i < stats.actions.Count; i++)
        {
            output = stats.actions[i].CombatAction(bpi, false);
            if (output.valid) return output;
            /* Outdated
            Node check = stats.actions[i].ActionValid(bpi, false);
            if (check != null)
            {
                CombatAction newAction = new CombatAction(this, check, stats.actions[i]);
                newAction.bpi = bpi;
                flag = true;
                output = newAction;
                break;
            } */
        }
        
            //Debug.LogError("No Available Actions for " + bpi.origin.stats.characterName);
        output = new CombatAction(this, pass);
        return output;        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerId == -2)
        {
            if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
            {
                CharacterSheet.ShowSheet(this);
                HighlightAction();
            }
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == -2)
        {
            CombatGrid.StopHighlight();
        }
    }

    public void HighlightAction()
    {
        CombatGrid.HighlightNodeStatic(movement.currentNode);

        if (stats.characterType == CharacterStats.CharacterTypes.Adventurer)
        {
            CombatGrid.instance.HighlighAction(currentAction);
        }
        else
        {
            Action a = currentAction.action;
            Action ga = null;
            for (int i = 0; i < entry.actionChecks.Count; i++)
            {
                if (entry.actionChecks[i].descriptionCorrect)
                {
                    if (entry.actionChecks[i].originalAction != null && entry.actionChecks[i].originalAction.actionCode.Equals(a.actionCode))
                    {
                        ga = entry.actionChecks[i].guessAction;
                    }
                }
            }
            if (ga != null)
            {
                if (ga.targetingSet)
                {
                    CombatAction ca = ga.CombatAction(new BattlefieldPositionInfo(this, CombatManager.characterPositions), true);
                        //new CombatAction(this, ga.ActionValid(new BattlefieldPositionInfo(this, CombatManager.characterPositions), true), ga);
                    ca.affectedNodes = CombatGrid.NodesAffectedByAction(ca);
                    CombatGrid.instance.HighlighAction(ca);
                }
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
        {
            HighlightAction();
        }
        CharacterSheet.ShowSheet(this);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (CombatManager.instance.currentStage == CombatManager.CombatStage.Setup)
            CombatGrid.StopHighlight();
    }
}
