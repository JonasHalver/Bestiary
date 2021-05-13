using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionManager : MonoBehaviour
{
    public Character character;
    private List<Action.Condition> conditions = new List<Action.Condition>();
    public Dictionary<Action.Condition, int> Conditions
    {
        get { return activeConditions; }
    }

    private Dictionary<Action.Condition, CombatManager.CombatTiming> expirationTimings = new Dictionary<Action.Condition, CombatManager.CombatTiming>()
    {
        { Action.Condition.Acid, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.Armor, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.Bleeding, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.Burning, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.DisorientMerc, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.DisorientMonster, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.Dodge, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.FearMerc, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.FearMonster, CombatManager.CombatTiming.EndOfRound },
        { Action.Condition.Haste, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.Poison, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.Regeneration, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.RootMerc, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.RootMonster, CombatManager.CombatTiming.EndOfRound },
        { Action.Condition.SlowMerc, CombatManager.CombatTiming.EndOfRound },
        { Action.Condition.SlowMonster, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.StrengthenSelf, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.StrengthenOther, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.Stun, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.TauntMerc, CombatManager.CombatTiming.StartOfCharacterTurn },
        { Action.Condition.TauntMonster, CombatManager.CombatTiming.EndOfRound },
        { Action.Condition.Vulnerable, CombatManager.CombatTiming.EndOfCharacterTurn },
        { Action.Condition.Weaken, CombatManager.CombatTiming.EndOfCharacterTurn }
    };
    private Dictionary<Action.Condition, bool> ignoreExpirationOnRoundApplied = new Dictionary<Action.Condition, bool>()
    {
        { Action.Condition.Acid, false },
        { Action.Condition.Armor, false },
        { Action.Condition.Bleeding, false },
        { Action.Condition.Burning, false },
        { Action.Condition.DisorientMerc, true },
        { Action.Condition.DisorientMonster, true },
        { Action.Condition.Dodge, false },
        { Action.Condition.FearMerc, true },
        { Action.Condition.FearMonster, false },
        { Action.Condition.Haste, true },
        { Action.Condition.Poison, false },
        { Action.Condition.Regeneration, false },
        { Action.Condition.RootMerc, true },
        { Action.Condition.RootMonster, false },
        { Action.Condition.SlowMerc, true },
        { Action.Condition.SlowMonster, true },
        { Action.Condition.StrengthenSelf, true},
        { Action.Condition.StrengthenOther, false },
        { Action.Condition.Stun, false },
        { Action.Condition.TauntMerc, true },
        { Action.Condition.TauntMonster, false },
        { Action.Condition.Vulnerable, false },
        { Action.Condition.Weaken, true }
    };
    private Dictionary<Action.Condition, bool> appliedThisRound = new Dictionary<Action.Condition, bool>()
    {
        { Action.Condition.Acid, false },
        { Action.Condition.Armor, false },
        { Action.Condition.Bleeding, false },
        { Action.Condition.Burning, false },
        { Action.Condition.DisorientMerc, false },
        { Action.Condition.DisorientMonster, false },
        { Action.Condition.Dodge, false },
        { Action.Condition.FearMerc, false },
        { Action.Condition.FearMonster, false },
        { Action.Condition.Haste, false },
        { Action.Condition.Poison, false },
        { Action.Condition.Regeneration, false },
        { Action.Condition.RootMerc, false },
        { Action.Condition.RootMonster, false },
        { Action.Condition.SlowMerc, false },
        { Action.Condition.SlowMonster, false },
        { Action.Condition.StrengthenSelf, false },
        { Action.Condition.StrengthenOther, false },
        { Action.Condition.Stun, false },
        { Action.Condition.TauntMerc, false },
        { Action.Condition.TauntMonster, false },
        { Action.Condition.Vulnerable, false },
        { Action.Condition.Weaken, false }
    };

    private Dictionary<Action.Condition, int> activeConditions = new Dictionary<Action.Condition, int>();

    public event System.Action<Action.Condition> GainedCondition;
    public event System.Action<Action.Condition> LostCondition;

    public void ApplyCondition(Action.Condition newCondition, int duration)
    {
        if (!activeConditions.ContainsKey(newCondition))
        {
            switch (newCondition)
            {
                default:
                    GainCondition(newCondition, duration);
                    break;
                case Action.Condition.Armor:
                    CheckMutuallyExclusiveConditions(Action.Condition.Vulnerable, newCondition, duration);
                    break;
                case Action.Condition.Vulnerable:
                    CheckMutuallyExclusiveConditions(Action.Condition.Armor, newCondition, duration);
                    break;
                case Action.Condition.SlowMerc:
                case Action.Condition.SlowMonster:
                    CheckMutuallyExclusiveConditions(Action.Condition.Haste, newCondition, duration);
                    break;
                case Action.Condition.Haste:
                    CheckMutuallyExclusiveConditions(Action.Condition.SlowMerc, Action.Condition.SlowMonster, newCondition, duration);
                    break;
                case Action.Condition.FearMerc:
                case Action.Condition.FearMonster:
                    CheckMutuallyExclusiveConditions(Action.Condition.TauntMerc, Action.Condition.TauntMonster, newCondition, duration);
                    break;
                case Action.Condition.TauntMerc:
                case Action.Condition.TauntMonster:
                    CheckMutuallyExclusiveConditions(Action.Condition.FearMonster, Action.Condition.FearMerc, newCondition, duration);
                    break;
            }
        }
        else
        {
            activeConditions[newCondition] += duration;
        }
    }

    public void TriggerOverTimeEffects()
    {
        if (activeConditions.ContainsKey(Action.Condition.Regeneration)) character.ReceiveHealing(true);
        foreach(KeyValuePair<Action.Condition, int> kvp in activeConditions)
        {
            if (Action.ConditionIsDamageOverTime[kvp.Key]) character.ReceiveHit(Action.DamageOverTimeType[kvp.Key]);
        }
    }

    public void UpdateDurations(CombatManager.CombatTiming timing)
    {
        if (timing == CombatManager.CombatTiming.StartOfNewRound)
        {
            foreach(KeyValuePair<Action.Condition, CombatManager.CombatTiming> kvp in expirationTimings) { appliedThisRound[kvp.Key] = false; }
        }
        foreach(KeyValuePair<Action.Condition, CombatManager.CombatTiming> kvp in expirationTimings)
        {
            if (kvp.Value == timing && activeConditions.ContainsKey(kvp.Key))
            {
                if (appliedThisRound[kvp.Key] && ignoreExpirationOnRoundApplied[kvp.Key]) continue;

                activeConditions[kvp.Key]--;
                if (activeConditions[kvp.Key] < 1)
                {
                    RemoveCondition(kvp.Key);
                }                
            }
        }
    }

    private void GainCondition(Action.Condition newCondition, int duration)
    {
        conditions.Add(newCondition);
        activeConditions.Add(newCondition, duration);
        appliedThisRound[newCondition] = true;
        GainedCondition.Invoke(newCondition);
    }

    private void RemoveCondition(Action.Condition condition)
    {
        conditions.Remove(condition);
        appliedThisRound[condition] = false;
        activeConditions.Remove(condition);
        LostCondition.Invoke(condition);
        if (condition == Action.Condition.TauntMonster || condition == Action.Condition.TauntMerc) character.Taunter = null;
        else if (condition == Action.Condition.FearMerc || condition == Action.Condition.FearMonster) character.AfraidOf.Clear();
    }

    private void CheckMutuallyExclusiveConditions(Action.Condition have, Action.Condition gained, int duration)
    {
        int oldDur, newDur;
        if (activeConditions.ContainsKey(have))
        {
            oldDur = activeConditions[have];
            newDur = oldDur - duration;
            if (newDur == 0)
            {
                RemoveCondition(have);
            }
            else if (newDur < 0)
            {
                newDur = Mathf.Abs(newDur);
                GainCondition(gained, newDur);
            }
        }
        else
        {
            GainCondition(gained, duration);
        }
    }
    private void CheckMutuallyExclusiveConditions(Action.Condition have, Action.Condition orHave, Action.Condition gained, int duration)
    {
        int oldDur, newDur;
        if (activeConditions.ContainsKey(have))
        {
            oldDur = activeConditions[have];
            newDur = oldDur - duration;
            if (newDur == 0)
            {
                RemoveCondition(have);
            }
            else if (newDur < 0)
            {
                newDur = Mathf.Abs(newDur);
                GainCondition(gained, newDur);
            }
        }
        else if (activeConditions.ContainsKey(orHave))
        {
            oldDur = activeConditions[orHave];
            newDur = oldDur - duration;
            if (newDur == 0)
            {
                RemoveCondition(orHave);
            }
            else if (newDur < 0)
            {
                newDur = Mathf.Abs(newDur);
                GainCondition(gained, newDur);
            }
        }
        else
        {
            GainCondition(gained, duration);
        }
    }
}
