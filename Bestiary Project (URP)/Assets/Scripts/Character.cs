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
    public float currentHitpoints;
    public bool alive = true;
    public AdventurerMovement movement;
    public Vector2 position;
    public List<Debuff> debuffs = new List<Debuff>();
    public List<Buff> buffs = new List<Buff>();
    public CombatAction currentAction;

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

    private Animator anim;
    private float t;
    public AnimationCurve attackAnimation;

    // Start is called before the first frame update
    void Start()
    {
        anim = transform.GetChild(0).GetComponent<Animator>();
        movement = GetComponent<AdventurerMovement>();

        if (!CombatManager.actors.Contains(this)) CombatManager.actors.Add(this);
        stats.actions.Sort((action1, action2) => action1.actionPriority.CompareTo(action2.actionPriority));

        currentHitpoints = stats.hitPoints;

    }

    // Update is called once per frame
    void Update()
    {
        if (currentHitpoints == 0) alive = false;
        if (!alive) deadImage.enabled = true;
        UpdatePosition();
        currentHitpoints = Mathf.Clamp(currentHitpoints, 0, stats.hitPoints);
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

    public void UpdateBuffsAndDebuffs()
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
        switch (interaction.action.actionType)
        {
            case Action.ActionType.Attack:
                if (!temporaryDodge)
                    ReceiveHit(interaction.action, false);
                break;
            case Action.ActionType.Healing:
                ReceiveHealing(interaction.action);
                break;
            case Action.ActionType.Buff:

                Buff buff = ScriptableObject.CreateInstance<Buff>();
                buff.SetValues(interaction.buff);
                buff.durationRemaining += (int)interaction.value;
                buff.affectedCharacter = this;
                bool flag = false;
                foreach (Buff d in buffs)
                {
                    if (d.buffType == buff.buffType)
                    {
                        d.durationRemaining += buff.duration;
                        flag = true;
                    }
                }
                if (!flag)
                {
                    buffs.Add(buff);
                    buff.ApplyBuff();
                    AcquiredBuff.Invoke(buff);
                }
                break;
            case Action.ActionType.Debuff:
                
                Debuff debuff = ScriptableObject.CreateInstance<Debuff>();
                debuff.SetValues(interaction.debuff, interaction.action.value, this);
                bool flag1 = false;
                foreach (Debuff d in debuffs)
                {
                    if (d.debuffType == debuff.debuffType)
                    {
                        if (d.debuffType == Debuff.DebuffType.Control && d.controlType == debuff.controlType)
                        {
                            d.durationRemaining += debuff.duration;
                            flag1 = true;
                        }
                        else if (d.debuffType == Debuff.DebuffType.DamageOverTime && d.damageType == debuff.damageType)
                        {
                            d.durationRemaining += debuff.duration;
                            flag = true;
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
        float damage = action.value;
        if (stats.armored || temporaryArmor) damage /= 2;
        if (stats.resistances.Contains(action.damageType)) damage /= 2;
        if (action.isCritical || critical || stats.weaknesses.Contains(action.damageType)) damage *= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
    }

    public void ReceiveHit(Debuff debuff)
    {
        float damage = .5f;
        if (stats.armored || temporaryArmor) damage /= 2;
        if (stats.resistances.Contains(debuff.damageType)) damage /= 2;
        if (stats.weaknesses.Contains(debuff.damageType)) damage *= 2;
        if (damage < 0.5f) damage = 0;
        currentHitpoints -= damage;
    }

    public void ReceiveHealing(Action action)
    {
        float healing = action.value;

        currentHitpoints += healing;
    }
    
    public void ReceiveHealing(Buff buff)
    {
        float healing = .5f;

        currentHitpoints += healing;
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
            Node check = stats.actions[i].ActionValid(bpi);
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
            CombatGrid.instance.HighlighAction(currentAction);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == -2)
        {
            CombatGrid.StopHighlight();
        }
    }
}
