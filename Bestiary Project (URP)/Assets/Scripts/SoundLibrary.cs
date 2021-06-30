using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Library", menuName = "Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [Header("Character")]
    public AudioClip pickUpCharacter;
    public AudioClip placeCharacter;
    public AudioClip invalidPlacement;
    public AudioClip moveCharacter;
    public AudioClip startCombat;
    public AudioClip characterDeath;

    [Header("Book")]
    public AudioClip openBook;
    public AudioClip closeBook;
    public AudioClip flipRight;
    public AudioClip flipLeft;
    public AudioClip openEditing;
    public AudioClip drag;
    public AudioClip deletionWarning;
    public AudioClip toggles;
    public AudioClip scribble;

    [Header("Effects")]
    public AudioClip cutting;
    public AudioClip crushing;
    public AudioClip piercing;
    public AudioClip acid;
    public AudioClip fire;
    public AudioClip cold;
    public AudioClip poison;
    public AudioClip healing;
    public AudioClip applyBuff;
    public AudioClip applyDebuff;
    public AudioClip pass;
}
