using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class Character : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum DamageTypes { Cutting, Piercing, Crushing, Fire, Acid, Cold, Poison, Healing }
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

    public List<Debuff.ControlType> conditions = new List<Debuff.ControlType>();

    [HideInInspector]
    public CombatAction lastCombatAction;
    [HideInInspector]
    public List<Debuff> expiredDebuffs = new List<Debuff>();
    [HideInInspector]
    public List<Buff> expiredBuffs = new List<Buff>();
    [HideInInspector]
    public List<DamageTypes> temporaryResistances = new List<DamageTypes>();
    public bool temporaryArmor = false;
    public bool temporaryDodge = false;
    public int initiative;

    private Animator anim;
    private float t;
    public AnimationCurve attackAnimation;

    public event System.Action TookDamage;
    public event System.Action Healed;

    public int debuffCount, buffCount;

    [HideInInspector] public Entry entry;
    [HideInInspector] public bool highlightMyNode = false;

    private void Awake()
    {
        if (stats == null) CreateEmptyStats();
    }
    public void CreateEmptyStats()
    {
        CharacterStats newStats = ScriptableObject.CreateInstance<CharacterStats>();
        stats = newStats;
        Action newAction = ScriptableObject.CreateInstance<Action>();
        stats.actions.Add(newAction); stats.actions.Add(newAction); stats.actions.Add(newAction); stats.actions.Add(newAction);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        CombatManager.StartOfTurn += StartOfTurn;
        CombatManager.EndOfTurn += EndOfTurn;
        CombatManager.EndOfMovement += EndOfMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHitpoints == 0) alive = false;
        if (!alive) deadImage.enabled = true;
        UpdatePosition();
        currentHitpoints = Mathf.Clamp(currentHitpoints, 0, stats.hitPoints);
        initiative = conditions.Contains(Debuff.ControlType.Slow) ? stats.speed - 2 : stats.speed;

        buffCount = buffs.Count;
        debuffCount = debuffs.Count;

        damageTaken = Mathf.Clamp(damageTaken, 0, Mathf.Infinity);

        if (stats.characterType == CharacterStats.CharacterTypes.NPC) entry = stats.entry;


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
            if (stats.actions[i].descriptionIndex > -1) stats.actions[i].actionDescription = Book.instance.descriptionsList.descriptions[i];
        }
        characterIcon.sprite = stats.characterIcon;
        characterIcon.color = stats.characterIconColor;
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

    public void ResetStats()
    {
        temporaryArmor = false;
        temporaryDodge = false;
        temporaryResistances.Clear();
        conditions.Clear();
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
            debuffs.Remove(expiredDebuffs[i]);
            LostDebuff.Invoke(expiredDebuffs[i]);
        }
    }

    public void Interaction(Interaction interaction)
    {
        Debuff debuff = null; 
        Buff buff = null;
        bool flag = false;
        switch (interaction.action.actionType)
        {
            case Action.ActionType.Attack:
                if (!temporaryDodge)
                    ReceiveHit(interaction.action, interaction.action.isCritical);
                break;
            case Action.ActionType.AttackDebuff:
                if (temporaryDodge) return;

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
    
    public void ReceiveHit(Action action, bool critical)
    {
        float damage = 1;
        if (stats.armored || temporaryArmor) damage /= 2;
        if (stats.resistances.Contains(action.damageType)) damage /= 2;
        if (action.isCritical || critical || stats.weaknesses.Contains(action.damageType)) damage *= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
        damageTaken += damage;
        TookDamage.Invoke();
    }

    public void ReceiveHit(Debuff debuff)
    {
        float damage = .5f;
        if (stats.armored || temporaryArmor) damage /= 2;
        if (stats.resistances.Contains(debuff.damageType)) damage /= 2;
        if (stats.weaknesses.Contains(debuff.damageType)) damage *= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
        damageTaken += damage;
        TookDamage.Invoke();
    }

    public void ReceiveHealing(Action action)
    {
        float healing = action.value;

        currentHitpoints += healing;
        damageTaken -= healing;
        Healed.Invoke();
    }
    
    public void ReceiveHealing(Buff buff)
    {
        float healing = .5f;

        currentHitpoints += healing;
        damageTaken -= healing;
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
        CombatAction output = new CombatAction(this, null, null);
        bool flag = false;

        for (int i = 0; i < stats.actions.Count; i++)
        {
            Node check = stats.actions[i].ActionValid(bpi, false);
            if (check != null)
            {
                CombatAction newAction = new CombatAction(this, check, stats.actions[i]);
                newAction.bpi = bpi;
                flag = true;
                output = newAction;
                break;
            }
        }
        if (!flag)
        {
            Debug.LogError("No Available Actions for " + bpi.origin.stats.characterName);
            return null;
        }
        else
        {
            return output;
        }
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
                CombatAction ca = new CombatAction(this, ga.ActionValid(new BattlefieldPositionInfo(this, CombatManager.characterPositions), true), ga);
                ca.affectedNodes = CombatGrid.NodesAffectedByAction(ca);
                CombatGrid.instance.HighlighAction(ca);
            }
        }
    }
}
