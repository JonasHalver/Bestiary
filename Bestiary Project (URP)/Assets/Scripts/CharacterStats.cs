using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Character")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    public enum CharacterTypes { NPC, Adventurer }
    public CharacterTypes characterType = CharacterTypes.NPC;
    public float hitPoints;
    public bool armored;
    public List<Character.DamageTypes> resistances = new List<Character.DamageTypes>();
    public List<Character.DamageTypes> weaknesses = new List<Character.DamageTypes>();
    public int speed;
    public int movement;
    public List<Action> actions = new List<Action>();
}
