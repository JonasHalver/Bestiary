using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff", menuName = "Debuff")]
public class Debuff : ScriptableObject
{
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

    public bool removeAtEndOfRound = false;

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
    }

    public void DebuffApplied()
    {
        switch (debuffType)
        {
            case DebuffType.Control:

                affectedCharacter.conditions.Add(controlType);

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
                affectedCharacter.conditions.Add(controlType);
                               
                break;
            case DebuffType.DamageOverTime:
                affectedCharacter.ReceiveHit(this);

                break;
        }
    }

    public void CheckDuration(bool endOfRound)
    {

        if (endOfRound && removeAtEndOfRound)
            durationRemaining--;
        else if (!removeAtEndOfRound && !endOfRound) durationRemaining--;

        if (durationRemaining <= 0)
        {
            if (removeAtEndOfRound && endOfRound)
                affectedCharacter.expiredDebuffs.Add(this);
            else if (!removeAtEndOfRound)
                affectedCharacter.expiredDebuffs.Add(this);
        }
    }
}
