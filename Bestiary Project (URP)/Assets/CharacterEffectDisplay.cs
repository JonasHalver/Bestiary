using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterEffectDisplay : MonoBehaviour
{
    public float fadeSpeed = 3;
    public GameObject effectPrefab;
    public GameObject iconPrefab;

    public List<Effect> effects = new List<Effect>();
    public Sprite collisionIcon, halfHeart,fullHeart,emptyHeart;
    public float displayDuration = 2;
    
    public void DisplayEffect(Effect effect)
    {
        GameObject newEffect = Instantiate(effectPrefab, transform);
        effects.Add(effect);
        effect.go = newEffect;
        SpawnIcons(effect, newEffect.transform.GetChild(0));
        StartCoroutine(Delay(displayDuration, effect));
    }
    public void RemoveEffects(Effect effect)
    {
        for (int i = 0; i < effect.icons.Count; i++)
        {
            StartCoroutine(IconFade(effect.icons[i], false, effect));
        }
    }
    public void SpawnIcons(Effect effect, Transform t)
    {
        GameObject newIcon = Instantiate(iconPrefab, t);
        Image img = newIcon.GetComponent<Image>();
        effect.icons.Add(img);
        switch (effect.type)
        {
            case Effect.Type.Damage:
                img.sprite = GameManager.instance.currentIconCollection.GetIcon(effect.damageType).icon;
                img.color = GameManager.instance.currentIconCollection.GetIcon(effect.damageType).iconColor;
                break;
            case Effect.Type.Condition:
                img.sprite = GameManager.instance.currentIconCollection.GetIcon(effect.condition).icon;
                img.color = GameManager.instance.currentIconCollection.GetIcon(effect.condition).iconColor;
                break;
            case Effect.Type.Collision:
                img.sprite = collisionIcon;
                img.color = Color.white;
                break;
            case Effect.Type.DamageOverTime:
                img.sprite = GameManager.instance.currentIconCollection.GetIcon(effect.condition).icon;
                img.color = GameManager.instance.currentIconCollection.GetIcon(effect.condition).iconColor;
                break;
        }
        StartCoroutine(IconFade(img, true, effect));
        if (effect.type != Effect.Type.Condition)
        {
            print(effect.value);
            float displayed = 0;
            bool half = false;
            if (effect.value % 1 == 0.5)
            {
                half = true;
                effect.value -= 0.5f;
            }
            else if (effect.value == 0)
            {
                newIcon = Instantiate(iconPrefab, t);
                img = newIcon.GetComponent<Image>();
                img.sprite = emptyHeart;
                img.color = Color.red;
                effect.icons.Add(img);
                StartCoroutine(IconFade(img, true, effect));
            }
            for (int i = 0; i < effect.value; i++)
            {
                newIcon = Instantiate(iconPrefab, t);
                img = newIcon.GetComponent<Image>();
                img.sprite = fullHeart;
                img.color = Color.red;
                effect.icons.Add(img);
                StartCoroutine(IconFade(img, true, effect));
                displayed++;
            }
            
            if (half)
            {
                newIcon = Instantiate(iconPrefab, t);
                img = newIcon.GetComponent<Image>();
                img.sprite = halfHeart;
                img.color = Color.red;
                effect.icons.Add(img);
                StartCoroutine(IconFade(img, true, effect));
                displayed += 0.5f;
            }
            print(displayed);
        }
    }
    IEnumerator IconFade(Image img, bool fadeIn, Effect effect)
    {
        float t = 0;
        while (t < 1)
        {
            img.color = new Color(img.color.r, img.color.g, img.color.b, fadeIn ? t : 1 - t);
            t += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        if (!fadeIn && effects.Contains(effect))
            RemoveEffect(effect);
    }
    public void RemoveEffect(Effect effect)
    {
        effects.Remove(effect);
        Destroy(effect.go);
    }
    IEnumerator Delay(float seconds, Effect effect)
    {
        float t = 0;
        while (t < seconds)
        {
            t += Time.deltaTime;
            while (GameManager.gameState != GameManager.GameState.Normal)
            {
                yield return null;
            }
            yield return null;
        }
        RemoveEffects(effect);
    }
}
public class Effect
{
    public float value;
    public Character.DamageTypes damageType;
    public Action.Condition condition;
    public bool fromCollision = false;
    public enum Type { Damage, Collision, Condition, DamageOverTime }
    public Type type;
    public List<Image> icons = new List<Image>();
    public GameObject go;
    // For damage
    public Effect(float _v, Character.DamageTypes _dt)
    {
        value = _v;
        damageType = _dt;
        type = Type.Damage;
    }
    // For collisions
    public Effect (bool _c, float _v)
    {
        value = _v;
        fromCollision = _c;
        type = Type.Collision;
    }
    // For condition application
    public Effect (Action.Condition _c)
    {
        condition = _c;
        type = Type.Condition;
    }
    // For damage over time
    public Effect (Action.Condition _c, float _v)
    {
        condition = _c;
        value = _v;
        type = Type.DamageOverTime;
    }
}
