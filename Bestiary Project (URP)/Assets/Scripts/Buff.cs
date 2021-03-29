using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Buff", menuName ="Buff")]
public class Buff : ScriptableObject
{
    public string buffName;
    public string tooltipString;

    public Character affectedCharacter;
    public enum BuffType { Armor, Resistance, Speed, Dodge, Regeneration }
    public BuffType buffType;
    public List<Character.DamageTypes> resistances = new List<Character.DamageTypes>();
    public int duration;
    public int durationRemaining = 0;

    public Sprite icon;
    public Color iconColor;

    public bool removeAtEndOfRound = false;
    public Debuff.EffectTiming effectTiming = Debuff.EffectTiming.StartOfTurn;
    public bool ignoreFirstInstance;
    private bool firstInstance = true;
    

    public Buff (BuffType _buffType, int _duration)
    {
        duration = _duration;
        durationRemaining += _duration;
        buffType = _buffType;
    }

    public void SetValues(Buff buff)
    {
        buffType = buff.buffType;
        resistances = buff.resistances;
        duration = buff.duration;
        icon = buff.icon;
        iconColor = buff.iconColor;
        removeAtEndOfRound = buff.removeAtEndOfRound;
        effectTiming = buff.effectTiming;
        tooltipString = buff.tooltipString;
        ignoreFirstInstance = buff.ignoreFirstInstance;
    }

    public void ApplyBuff(bool onApplication)
    {
        switch (buffType)
        {
            case BuffType.Armor:
                affectedCharacter.currentBuffs.Add(buffType);
                break;
            case BuffType.Resistance:
                for (int i = 0; i < resistances.Count; i++)
                {
                    affectedCharacter.temporaryResistances.Add(resistances[i]);
                }
                break;
            case BuffType.Speed:
                affectedCharacter.currentBuffs.Add(buffType);
                break;
            case BuffType.Dodge:
                affectedCharacter.currentBuffs.Add(buffType);
                break;
            case BuffType.Regeneration:
                if (!onApplication)
                affectedCharacter.ReceiveHealing(this);
                break;
        }
    }

    public void ResolveBuff()
    {
        ApplyBuff(false);        
    }

    public void CheckDuration(Debuff.EffectTiming timing)
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
            affectedCharacter.expiredBuffs.Add(this);
        }

        //if (endOfRound && removeAtEndOfRound)
        //    durationRemaining--;
        //else
        //    durationRemaining--;
        //
        //if (durationRemaining <= 0)
        //{
        //    if (removeAtEndOfRound && endOfRound)
        //        affectedCharacter.expiredBuffs.Add(this);
        //    else if (!removeAtEndOfRound)
        //        affectedCharacter.expiredBuffs.Add(this);
        //}
    }
}
