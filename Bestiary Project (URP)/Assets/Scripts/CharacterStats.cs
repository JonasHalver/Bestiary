using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(fileName = "New Character Stats", menuName = "Character")]
public class CharacterStats : ScriptableObject
{
    public string characterName;
    [Tooltip("Make this a random string")]
    public string characterCode;
    public enum CharacterTypes { NPC, Adventurer }
    public CharacterTypes characterType = CharacterTypes.NPC;
    public enum Personality { Aggressive, Annoying, Cunning, Efficient, Fearful, PainAverse, Reckless, Relentless }
    public Personality personality;
    public Sprite characterIcon;
    public Color characterIconColor;
    public float hitPoints = 1;
    public bool armored;
    public List<Character.DamageTypes> resistances = new List<Character.DamageTypes>();
    public List<Character.DamageTypes> weaknesses = new List<Character.DamageTypes>();
    public int speed = 5;
    public int movement = 2;
    public List<Action> actions = new List<Action>();
    public int pageNumber;
    [HideInInspector]
    public Entry entry;

}
