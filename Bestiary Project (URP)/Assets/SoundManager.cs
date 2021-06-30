using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioSource characterSound;
    public AudioSource bookSound;
    public AudioSource effects;
    public AudioSource dragging;
    public AudioSource writing;
    public SoundLibrary library;

    public List<AudioClip> activeClips = new List<AudioClip>();

    private void Awake()
    {
        instance = this;
    }

    // Character
    public static void PickupCharacter()
    {
        instance.StopCharacterPlay();
        instance.characterSound.PlayOneShot(instance.library.pickUpCharacter);
    }
    public static void PlaceCharacter()
    {
        instance.StopCharacterPlay();
        instance.characterSound.PlayOneShot(instance.library.placeCharacter);
    }
    public static void MoveCharacter()
    {
        instance.StopCharacterPlay();
        instance.characterSound.PlayOneShot(instance.library.moveCharacter);
    }
    public static void InvalidPlacement()
    {
        instance.StopCharacterPlay();
        instance.characterSound.PlayOneShot(instance.library.invalidPlacement);
    }
    public static void StartCombat()
    {
        instance.StopCharacterPlay();
        instance.characterSound.PlayOneShot(instance.library.startCombat);
    }
    private void StopCharacterPlay()
    {
        //characterSound.Stop();
    }

    // Book
    public static void OpenBook()
    {
        instance.StopBookPlay();
        instance.bookSound.pitch = 1;
        instance.bookSound.PlayOneShot(instance.library.openBook);
    }
    public static void CloseBook()
    {
        instance.bookSound.pitch = 1;
        instance.StopBookPlay();
        instance.bookSound.PlayOneShot(instance.library.closeBook);
    }
    public static void PageChange(bool forward)
    {
        instance.StopBookPlay();
        instance.bookSound.pitch = 1 + (forward ? .1f : -.1f);
        instance.bookSound.PlayOneShot(instance.library.flipLeft);
    }
    public static void ChapterChange()
    {
        instance.bookSound.pitch = 1;
        instance.StopBookPlay();
        instance.bookSound.PlayOneShot(instance.library.flipRight);
    }
    public static void OpenEditing()
    {
        instance.bookSound.pitch = 1;
        instance.StopBookPlay();
        instance.bookSound.PlayOneShot(instance.library.openEditing);
    }
    public static void DeletionWarning()
    {
        instance.bookSound.pitch = 1;
        instance.bookSound.PlayOneShot(instance.library.deletionWarning);
    }
    private void StopBookPlay()
    {
        //bookSound.Stop();
    }

    // Effect
    public static void DamageSoundEffect(Character.DamageTypes damageType)
    {
        AudioClip output = null;
        switch (damageType)
        {
            case Character.DamageTypes.Cutting:
                output = instance.library.cutting;
                break;
            case Character.DamageTypes.Crushing:
                output = instance.library.crushing;
                break;
            case Character.DamageTypes.Piercing:
                output = instance.library.piercing;
                break;
            case Character.DamageTypes.Fire:
                output = instance.library.fire;
                break;
            case Character.DamageTypes.Acid:
                output = instance.library.acid;
                break;
            case Character.DamageTypes.Cold:
                output = instance.library.cold;
                break;
            case Character.DamageTypes.Poison:
                output = instance.library.poison;
                break;
            case Character.DamageTypes.Healing:
                output = instance.library.healing;
                break;
        }
        if (instance.activeClips.Count > 0)
        {
            if (instance.activeClips[instance.activeClips.Count - 1] == output) return;
        }
        if (output != null) instance.activeClips.Add(output);
    }
    public static void BuffSoundEffect()
    {
        if (instance.activeClips.Count > 0)
        {
            if (instance.activeClips[instance.activeClips.Count - 1] == instance.library.applyBuff) return;
        }
        instance.activeClips.Add(instance.library.applyBuff);
    }
    public static void DebuffSoundEffect()
    {
        if (instance.activeClips.Count > 0)
        {
            if (instance.activeClips[instance.activeClips.Count - 1] == instance.library.applyDebuff) return;
        }
        instance.activeClips.Add(instance.library.applyDebuff);
    }
    public static void PassEffect()
    {
        if (instance.activeClips.Count > 0)
        {
            if (instance.activeClips[instance.activeClips.Count - 1] == instance.library.pass) return;
        }
        instance.activeClips.Add(instance.library.pass);

    }
    private void Update()
    {
        if (!effects.isPlaying)
        {
            if (activeClips.Count > 0)
            {
                effects.clip = activeClips[0];
                effects.Play();
                activeClips.RemoveAt(0);
            }
        }
    }

    // Dragging
    public static void Drag(float velocity)
    {
        if (!instance.dragging.isPlaying) instance.dragging.Play();
        float t = Mathf.InverseLerp(0, 2500, velocity);
        instance.dragging.volume = Mathf.Lerp(0, 0.5f, t);
        instance.dragging.pitch = 1 + Mathf.Lerp(-.8f, .2f, t);
    }
    public static void StopDrag()
    {
        instance.dragging.Stop();
    }

    public static void Scribble()
    {
        instance.writing.PlayOneShot(instance.library.scribble);
    }
}
