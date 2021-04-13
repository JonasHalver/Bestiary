using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff", menuName = "Debuff")]
public class Debuff : ScriptableObject
{
    public string debuffName;
    public string bookDescription;
    public string tooltipString;
    public Character affectedCharacter;

    public enum DebuffType { DamageOverTime, Control }
    public enum ControlType { Slow, Root, Blind }
    public DebuffType debuffType;
    public ControlType controlType;

    public Sprite icon;
    public Color iconColor;

    public int duration;
    public int durationRemaining;
    public Character.DamageTypes damageType;
    
    public enum EffectTiming { StartOfTurn, EndOfTurn, Movement }
    public EffectTiming effectTiming = EffectTiming.StartOfTurn;

    public bool removeAtEndOfRound = false;
    public bool ignoreFirstInstance = false;
    private bool firstInstance = true;

    public Debuff(DebuffType _type, Character.DamageTypes _damageType, int _duration)
    {
        debuffType = _type;
        damageType = _damageType;
        duration = _duration;
        durationRemaining += _duration;
    }

    public Debuff(DebuffType _type, ControlType _controlType, int _duration)
    {
        debuffType = _type;
        controlType = _controlType;
        duration = _duration;
        durationRemaining += _duration;
    }

    public void SetValues(Debuff _debuff, int _duration, Character target)
    {
        debuffType = _debuff.debuffType;
        controlType = _debuff.controlType;
        damageType = _debuff.damageType;
        affectedCharacter = target;
        icon = _debuff.icon;
        iconColor = _debuff.iconColor;
        duration = _duration;
        durationRemaining += _duration;
        removeAtEndOfRound = _debuff.removeAtEndOfRound;
        effectTiming = _debuff.effectTiming;
        tooltipString = _debuff.tooltipString;
        ignoreFirstInstance = _debuff.ignoreFirstInstance;
    }

    public void DebuffApplied()
    {
        switch (debuffType)
        {
            case DebuffType.Control:

                //affectedCharacter.conditions.Add(controlType);

                break;
            case DebuffType.DamageOverTime:
                
                break;
        }
    }

    public void ResolveDebuff()
    {

        switch (debuffType)
        {
            case DebuffType.Control:
                //affectedCharacter.conditions.Add(controlType);
                               
                break;
            case DebuffType.DamageOverTime:
                //affectedCharacter.ReceiveHit(this);

                break;
        }
    }

    public void RemoveDebuff()
    {
        switch (debuffType)
        {
            case DebuffType.Control:
                //if (affectedCharacter.conditions.Contains(controlType)) affectedCharacter.conditions.Remove(controlType);
                break;
        }
    }

    public void CheckDuration(EffectTiming timing)
    {
        if (ignoreFirstInstance && firstInstance)
        {
            firstInstance = false;
            return;
        }
        if (timing == effectTiming)
        {
            durationRemaining--;
        }
        if (durationRemaining <= 0)
        {
            affectedCharacter.expiredDebuffs.Add(this);
        }

        //if (endOfRound && removeAtEndOfRound)
        //    durationRemaining--;
        //else if (!removeAtEndOfRound && !endOfRound) durationRemaining--;
        //
        //if (durationRemaining <= 0)
        //{
        //    if (removeAtEndOfRound && endOfRound)
        //        affectedCharacter.expiredDebuffs.Add(this);
        //    else if (!removeAtEndOfRound)
        //        affectedCharacter.expiredDebuffs.Add(this);
        //}
    }
}
