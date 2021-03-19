using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Encounter", menuName = "Combat Encounter")]
public class CombatEncounter : ScriptableObject
{
    public enum EncounterDifficulty { Easy, Normal, Dangerous }
    public EncounterDifficulty difficulty = EncounterDifficulty.Normal;
    public List<CharacterStats> enemies = new List<CharacterStats>();
}
