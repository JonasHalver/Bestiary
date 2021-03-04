using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Debuff", menuName = "Debuff")]
public class Debuff : ScriptableObject
{
    public Character affectedCharacter;

    public enum DebuffType { DamageOverTime, Control }
    public enum ControlType { Slow, Stun, Blind }
    public DebuffType debuffType;
    public ControlType controlType;

    public Sprite icon;
    public Color iconColor;

    public int duration;
    public int durationRemaining;
    public Character.DamageTypes damageType;

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
                if (durationRemaining <= 0)
                {
                    affectedCharacter.expiredDebuffs.Add(this);
                }
                else
                {
                    affectedCharacter.conditions.Add(controlType);
                    durationRemaining--;
                }
                break;
            case DebuffType.DamageOverTime:
                affectedCharacter.ReceiveHit(this);
                durationRemaining--;
                if (durationRemaining <= 0)
                {
                    affectedCharacter.expiredDebuffs.Add(this);
                }
                break;
        }
    }
}
