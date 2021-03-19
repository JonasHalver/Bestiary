using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Icon Collection", menuName = "Icon Collection")]
public class Icons : ScriptableObject
{
    [Header("Damage Types")]
    public Sprite cutting;
    public Sprite crushing;
    public Sprite piercing;
    public Sprite fire;
    public Sprite acid;
    public Sprite cold;
    public Sprite poison;
    public Sprite healing;
    [Header("Shape Icons")]
    public Sprite self;
    public Sprite ranged;
    public Sprite meleeDia;
    public Sprite meleeVert;
    public Sprite arcDia;
    public Sprite arcVert;
    public Sprite coneDia;
    public Sprite coneVert;
    public Sprite lineDia;
    public Sprite lineVert;
    public Sprite box;
    public Sprite selfBox;
    [Header("Buff Icons")]
    public Sprite dodge;
    public Sprite regeneration;
    public Sprite armor;
    [Header("Debuff Icons")]
    public Sprite burning;
    public Sprite acidDebuff;
    public Sprite poisonDebuff;
    public Sprite root;
    public Sprite slow;
    public Sprite blind;
}
