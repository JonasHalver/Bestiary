using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Buff", menuName ="Buff")]
public class Buff : ScriptableObject
{
    public string buffName;

    public Character affectedCharacter;
    public enum BuffType { Armor, Resistance, Speed, Dodge }
    public BuffType buffType;
    public List<Character.DamageTypes> resistances = new List<Character.DamageTypes>();
    public int duration;
    public int durationRemaining = 0;

    public Sprite icon;
    public Color iconColor;

    public bool removeAtEndOfRound = false;
    public Debuff.EffectTiming effectTiming = Debuff.EffectTiming.StartOfTurn;

    

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
    }

    public void ApplyBuff()
    {
        switch (buffType)
        {
            case BuffType.Armor:
                affectedCharacter.temporaryArmor = true;
                break;
            case BuffType.Resistance:
                for (int i = 0; i < resistances.Count; i++)
                {
                    affectedCharacter.temporaryResistances.Add(resistances[i]);
                }
                break;
            case BuffType.Speed:
                break;
            case BuffType.Dodge:
                affectedCharacter.temporaryDodge = true;
                break;
        }
    }

    public void ResolveBuff()
    {
        ApplyBuff();        
    }

    public void CheckDuration(Debuff.EffectTiming timing)
    {
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
